#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using UnityEngine.Profiling;
using Unity.Profiling;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;
	using Pathfinding.Drawing;
	using Pathfinding.RVO;
	using Unity.Collections;
	using Unity.Burst.Intrinsics;
	using System.Diagnostics;

	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[BurstCompile]
	public partial struct FollowerControlSystem : ISystem {
		EntityQuery entityQueryControl;
		EntityQuery entityQueryControlManaged;
		EntityQuery entityQueryControlManaged2;
		RedrawScope redrawScope;

		static readonly ProfilerMarker MarkerMovementOverrideBeforeControl = new ProfilerMarker("MovementOverrideBeforeControl");
		static readonly ProfilerMarker MarkerMovementOverrideAfterControl = new ProfilerMarker("MovementOverrideAfterControl");

		public void OnCreate (ref SystemState state) {
			redrawScope = DrawingManager.GetRedrawScope();

			entityQueryControl = state.GetEntityQuery(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<AgentMovementPlane>(),
				ComponentType.ReadOnly<DestinationPoint>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadOnly<MovementStatistics>(),
				ComponentType.ReadWrite<ManagedState>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadOnly<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementControl>(),

				ComponentType.Exclude<AgentOffMeshLinkTraversal>(),
				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementControl>()
				);

			entityQueryControlManaged = state.GetEntityQuery(
				ComponentType.ReadWrite<ManagedMovementOverrideBeforeControl>(),

				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<AgentCylinderShape>(),
				ComponentType.ReadWrite<AgentMovementPlane>(),
				ComponentType.ReadWrite<DestinationPoint>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadWrite<MovementStatistics>(),
				ComponentType.ReadWrite<ManagedState>(),
				ComponentType.ReadWrite<MovementSettings>(),
				ComponentType.ReadWrite<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementControl>(),

				ComponentType.Exclude<AgentOffMeshLinkTraversal>(),
				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementControl>()
				);

			entityQueryControlManaged2 = state.GetEntityQuery(
				ComponentType.ReadWrite<ManagedMovementOverrideAfterControl>(),

				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<AgentCylinderShape>(),
				ComponentType.ReadWrite<AgentMovementPlane>(),
				ComponentType.ReadWrite<DestinationPoint>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadWrite<MovementStatistics>(),
				ComponentType.ReadWrite<ManagedState>(),
				ComponentType.ReadWrite<MovementSettings>(),
				ComponentType.ReadWrite<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementControl>(),

				ComponentType.Exclude<AgentOffMeshLinkTraversal>(),
				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementControl>()
				);
		}

		public void OnDestroy (ref SystemState state) {
			redrawScope.Dispose();
		}

		public void OnUpdate (ref SystemState systemState) {
			// The full movement calculations do not necessarily need to be done every frame if the fps is high
			if (AstarPath.active != null && !AIMovementSystemGroup.TimeScaledRateManager.CheapSimulationOnly) {
				ProcessControlLoop(ref systemState, SystemAPI.Time.DeltaTime);
			}
		}

		void ProcessControlLoop (ref SystemState systemState, float dt) {
			// This is a hook for other systems to modify the movement of agents.
			// Normally it is not used.
			if (!entityQueryControlManaged.IsEmpty) {
				MarkerMovementOverrideBeforeControl.Begin();
				systemState.Dependency.Complete();
				new JobManagedMovementOverrideBeforeControl {
					dt = dt,
				}.Run(entityQueryControlManaged);
				MarkerMovementOverrideBeforeControl.End();
			}

			redrawScope.Rewind();
			var draw = DrawingManager.GetBuilder(redrawScope);
			var navmeshEdgeData = AstarPath.active.GetNavmeshBorderData(out var readLock);
			systemState.Dependency = new JobControl {
				navmeshEdgeData = navmeshEdgeData,
				draw = draw,
				dt = dt,
			}.ScheduleParallel(entityQueryControl, JobHandle.CombineDependencies(systemState.Dependency, readLock.dependency));
			readLock.UnlockAfter(systemState.Dependency);
			draw.DisposeAfter(systemState.Dependency);

			if (!entityQueryControlManaged2.IsEmpty) {
				MarkerMovementOverrideAfterControl.Begin();
				systemState.Dependency.Complete();
				new JobManagedMovementOverrideAfterControl {
					dt = dt,
				}.Run(entityQueryControlManaged2);
				MarkerMovementOverrideAfterControl.End();
			}
		}
	}
}
#endif