﻿using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace Deform
{
	/// <summary>
	/// The implementation of IDeformable meant for deforming a MeshFilter or SkinnedMeshRenderer's mesh.
	/// </summary>
	[ExecuteAlways, DisallowMultipleComponent]
	[HelpURL("https://github.com/keenanwoodall/Deform/wiki/Deformable")]
	public class ElasticDeformable : Deformable
	{
		public enum VertexColorMask { None = -1, R = 0, G = 1, B = 2, A = 3 }
		
		public float DampingRatio
		{
			get => dampingRatio;
			set => dampingRatio = Mathf.Clamp01(value);
		}
		public float AngularFrequency
		{
			get => angularFrequency;
			set => angularFrequency = value;
		}

		public VertexColorMask Mask
		{
			get => mask;
			set => mask = value;
		}
		
		public float MaskedDampingRatio
		{
			get => maskedDampingRatio;
			set => maskedDampingRatio = Mathf.Clamp01(value);
		}
		public float MaskedAngularFrequency
		{
			get => maskedAngularFrequency;
			set => maskedAngularFrequency = value;
		}

		[Tooltip("A value of zero will result in infinite oscillation. A value of one will result in no oscillation.")]
		[SerializeField, Range(0f, 1f)] private float dampingRatio = 0.3f;
		[Tooltip("An angular frequency of 1 means the oscillation completes one full period over one second.")]
		[SerializeField] private float angularFrequency = 4f;
		[SerializeField] private Vector3 gravity = Vector3.zero;
		
		[SerializeField]
		private VertexColorMask mask = VertexColorMask.None;
		
		[Tooltip("A value of zero will result in infinite oscillation. A value of one will result in no oscillation.")]
		[SerializeField, Range(0f, 1f)] private float maskedDampingRatio = .8f;
		[Tooltip("An angular frequency of 1 means the oscillation completes one full period over one second.")]
		[SerializeField] private float maskedAngularFrequency = 8f;

		private NativeArray<float3> velocityBuffer;
		private NativeArray<float3> currentPointBuffer;

		public override UpdateFrequency UpdateFrequency => UpdateFrequency.Immediate;

		public override void InitializeData()
		{
			base.InitializeData();
			velocityBuffer = new NativeArray<float3>(data.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			currentPointBuffer = new NativeArray<float3>(data.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

			data.OriginalNative.VertexBuffer.CopyTo(currentPointBuffer);
			
			new TransformPointsJob
			{
				points = currentPointBuffer,
				matrix = transform.localToWorldMatrix
			}.Schedule(currentPointBuffer.Length, 128).Complete();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (velocityBuffer != null)
				velocityBuffer.Dispose();
			if (currentPointBuffer != null)
				currentPointBuffer.Dispose();
		}

		/// <summary>
		/// Creates a chain of work to deform the native mesh data.
		/// </summary>
		public override JobHandle Schedule(JobHandle dependency = default)
		{
			if (data.Target.GetGameObject() == null)
				if (!data.Initialize(gameObject))
					return dependency;

			// Don't try to process any data if we're disabled or our data is broken.
			if (!CanUpdate())
				return dependency;

			// We need to dispose of this objects data if it is destroyed.
			// We can't destroy the data if there's a job currently using it,
			// so we need to cache a reference to this objects part of the chain.
			// That will let us force this objects portion of the work to complete
			// which will let us dispose of its data and avoid a leak.
			handle = dependency;

			// Create a chain of job handles to process the data.
			for (int i = 0; i < deformerElements.Count; i++)
			{
				var element = deformerElements[i];
				var deformer = element.Component;

				// Only add the current deformer to the dependency chain if it wants to update.
				if (element.CanProcess())
				{
					// If this deformer need updated bounds, add bounds recalculation
					// to the end of the chain.
					if (deformer.RequiresUpdatedBounds && BoundsRecalculation == BoundsRecalculation.Auto)
					{
						handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
						currentModifiedDataFlags |= DataFlags.Bounds;
					}

					// Add the current deformer to the end of the dependency chain.
					handle = deformer.Process(data, handle);
					currentModifiedDataFlags |= deformer.DataFlags;
				}
			}

			if (Application.isPlaying)
			{
				if (!Mathf.Approximately(gravity.sqrMagnitude, 0f))
				{
					handle = new AddFloat3ToFloat3sJob
					{
						value = gravity * Time.deltaTime,
						values = velocityBuffer
					}.Schedule(velocityBuffer.Length, Deformer.DEFAULT_BATCH_COUNT, handle);
				}
				
				// After processing all deformers, the vertex buffer holds the desired end positions in local-space.
				// The elastic effect should be applied in world space, so the vertex buffer needs to be transformed
				// to worldspace.
				handle = new TransformPointsJob
				{
					points = data.DynamicNative.VertexBuffer,
					matrix = transform.localToWorldMatrix
				}.Schedule(data.Length, Deformer.DEFAULT_BATCH_COUNT, handle);
				// The current and target points are now in world-space. Apply elastic forces
				if (Mask == VertexColorMask.None)
				{
					handle = new ElasticPointsUpdateJob
					{
						dampingRatio = DampingRatio,
						angularFrequency = AngularFrequency,
						deltaTime = Time.deltaTime,
						velocities = velocityBuffer,
						currentPoints = currentPointBuffer,
						targetPoints = data.DynamicNative.VertexBuffer
					}.Schedule(data.Length, Deformer.DEFAULT_BATCH_COUNT, handle);
				}
				else
				{
					handle = new MaskedElasticPointsUpdateJob
					{
						unmaskedDampingRatio = DampingRatio,
						unmaskedAngularFrequency = AngularFrequency,
						maskedDampingRatio = maskedDampingRatio,
						maskedAngularFrequency =  maskedAngularFrequency,
						deltaTime = Time.deltaTime, 
						velocities = velocityBuffer,
						currentPoints = currentPointBuffer,
						targetPoints = data.DynamicNative.VertexBuffer,
						colors = data.DynamicNative.ColorBuffer,
						maskIndex = (int) Mask
					}.Schedule(data.Length, Deformer.DEFAULT_BATCH_COUNT, handle);;
				}

				// Before applying the mesh data, the current point buffer will be swapped with the vertex buffer
				// so the current point buffer needs to be transformed back to local-space.
				handle = new TransformPointsFromJob()
				{
					from = currentPointBuffer,
					to = data.DynamicNative.VertexBuffer,
					matrix = transform.worldToLocalMatrix
				}.Schedule(data.Length, 128, handle);
			}

			if (NormalsRecalculation == NormalsRecalculation.Auto)
			{
				// Add normal recalculation to the end of the deformation chain.
				if (Application.isPlaying)
					handle = MeshUtils.RecalculateNormals(data.DynamicNative, handle);
				else
					handle = MeshUtils.RecalculateNormals(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Normals;
			}
			if (BoundsRecalculation == BoundsRecalculation.Auto || BoundsRecalculation == BoundsRecalculation.OnceAtTheEnd)
			{
				// Add bounds recalculation to the end as well.
				if (Application.isPlaying)
					handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
				else
					handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Bounds;
			}

			// Return the new end of the dependency chain.
			return handle;
		}

		/// <summary>
		/// Sends native mesh data to the mesh, updates the mesh collider if required and then resets the native mesh data.
		/// </summary>
		public override void ApplyData()
		{
			if (!CanUpdate())
				return;

			// If in play-mode, always apply vertices since it's an elastic effect
			if (Application.isPlaying)
			{
				currentModifiedDataFlags |= DataFlags.Vertices;
			}

			data.ApplyData(currentModifiedDataFlags);

			if (BoundsRecalculation == BoundsRecalculation.Custom)
				data.DynamicMesh.bounds = CustomBounds;

			if (ColliderRecalculation == ColliderRecalculation.Auto)
				RecalculateMeshCollider();

			ResetDynamicData();
		}
	}
}