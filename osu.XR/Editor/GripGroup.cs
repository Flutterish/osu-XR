﻿using osu.Framework.XR.Components;
using osu.Framework.XR.Maths;
using osuTK;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace osu.XR.Editor {
	public class GripGroup {
		Dictionary<Drawable3D, GripEvent> gripSources = new();
		public IGripable? Target { get; private set; }

		public record GripEvent {
			/// <summary>
			/// The global start position of the grip event
			/// </summary>
			public Vector3 StartPosition;
			/// <summary>
			/// The global start rotation of the grip event
			/// </summary>
			public Quaternion StartRotation;
			/// <summary>
			/// The current global position of the grip event
			/// </summary>
			public Vector3 Position;
			/// <summary>
			/// The current global rotation of the grip event
			/// </summary>
			public Quaternion Rotation;

			/// <summary>
			/// The global start position of the target
			/// </summary>
			public Vector3 TargetStartPosition;
			/// <summary>
			/// The global start rotation of the target
			/// </summary>
			public Quaternion TargetStartRotation;
			/// <summary>
			/// The start scale of the target
			/// </summary>
			public Vector3 TargetStartScale;
		}

		public bool TryGrip ( Drawable3D target, Drawable3D source ) {
			if ( this.Target is not null && target != this.Target ) return false;

			if ( target is IGripable gripable && gripable.CanBeGripped ) {
				this.Target = gripable;
				if ( !gripSources.ContainsKey( source ) ) {
					updateInitialValues();
					gripSources.Add( source, new() {
						StartPosition = source.GlobalPosition,
						StartRotation = source.GlobalRotation,
						Position = source.GlobalPosition,
						Rotation = source.GlobalRotation,
						TargetStartPosition = target.GlobalPosition,
						TargetStartRotation = target.GlobalRotation,
						TargetStartScale = target.Scale
					} );

					gripable.OnGripped( source, this );
				}
				return true;
			}
			return false;
		}

		public void Release ( Drawable3D source ) {
			if ( Target is null ) return;

			gripSources.Remove( source );
			Target.OnGripReleased( source, this );
			updateInitialValues();
			if ( gripSources.Count == 0 ) Target = null;
		}

		void updateInitialValues () {
			if ( Target is not Drawable3D drawable ) return;
			foreach ( var (src, @event) in gripSources ) {
				gripSources[ src ] = @event with
				{
					StartPosition = @event.Position,
					StartRotation = @event.Rotation,
					TargetStartPosition = drawable.GlobalPosition,
					TargetStartRotation = drawable.GlobalRotation,
					TargetStartScale = drawable.Scale
				};
			}
		}

		public void Update () {
			if ( Target is not Drawable3D drawable ) return;
			
			foreach ( var (source,@event) in gripSources ) {
				gripSources[source] = @event with { 
					Position = source.GlobalPosition,
					Rotation = source.GlobalRotation
				};
			}

			if ( gripSources.Count == 0 ) {
				return;
			}
			else if ( gripSources.Count == 1 ) {
				var grip = gripSources.Values.Single();

				if ( Target.AllowsGripMovement && Target.AllowsGripRotation ) { // here we kind of hold it on a "stick"
					var offset = grip.TargetStartPosition - grip.StartPosition;
					var rot = grip.Rotation * grip.StartRotation.Inverted();

					drawable.GlobalPosition = grip.Position + (rot * new Vector4( offset, 1 )).Xyz;

					drawable.GlobalRotation = rot * grip.TargetStartRotation;
				}
				else if ( Target.AllowsGripMovement ) { // just move with the pointer
					drawable.GlobalPosition = grip.TargetStartPosition + ( grip.Position - grip.StartPosition );
				}
				else if ( Target.AllowsGripRotation ) { // rotate about the centre
					var initialDirection = ( grip.StartPosition - grip.TargetStartPosition ).Normalized();
					var currentDirection = ( grip.Position - grip.TargetStartPosition ).Normalized();

					drawable.GlobalRotation = initialDirection.ShortestRotationTo( currentDirection ) * grip.TargetStartRotation;
				}
			}
			else if ( gripSources.Count == 2 ) {
				var a = gripSources.Values.ElementAt( 0 );
				var b = gripSources.Values.ElementAt( 1 );

				if ( Target.AllowsGripRotation && Target.AllowsGripScaling && Target.AllowsGripMovement ) { // A grips and then rotate about A so you match B's direction, then scale about A to match B
					var offset = a.TargetStartPosition - a.StartPosition;
					var rot = ( b.StartPosition - a.StartPosition ).Normalized().ShortestRotationTo( ( b.Position - a.Position ).Normalized() );

					drawable.GlobalRotation = rot * a.TargetStartRotation;

					var agvStartScale = gripSources.Values.Average( a => a.TargetStartScale );

					drawable.Scale = agvStartScale * computeScaleMultiplier();

					drawable.GlobalPosition = a.Position + ( rot * new Vector4( offset, 1 ) ).Xyz * computeScaleMultiplier();
				}
				else if ( Target.AllowsGripRotation && Target.AllowsGripMovement ) { // A grips and then rotate about A so you match B's direction
					var offset = a.TargetStartPosition - a.StartPosition;
					var rot = ( b.StartPosition - a.StartPosition ).Normalized().ShortestRotationTo( ( b.Position - a.Position ).Normalized() );

					drawable.GlobalRotation = rot * a.TargetStartRotation;
					drawable.GlobalPosition = a.Position + ( rot * new Vector4( offset, 1 ) ).Xyz;
				}
				else {
					if ( Target.AllowsGripRotation ) { // rotate as if A was the centre of rotation. We cant move the target so we just pretend A is the target
						var rot = ( b.StartPosition - a.StartPosition ).Normalized().ShortestRotationTo( ( b.Position - a.Position ).Normalized() );

						drawable.GlobalRotation = rot * a.TargetStartRotation;
					}

					if ( Target.AllowsGripScaling ) { // scale as pointers move to/away from the baricentre
						var agvStartScale = gripSources.Values.Average( a => a.TargetStartScale );

						drawable.Scale = agvStartScale * computeScaleMultiplier();
					}

					if ( Target.AllowsGripMovement ) { // follow the average delta
						drawable.GlobalPosition = gripSources.Values.Average( x => x.TargetStartPosition + x.Position - x.StartPosition );
					}
				}
			}
			else {
				if ( Target.AllowsGripScaling ) { // scale as pointers move to/away from the baricentre
					var agvStartScale = gripSources.Values.Average( a => a.TargetStartScale );

					drawable.Scale = agvStartScale * computeScaleMultiplier();
				}
				
				if ( Target.AllowsGripMovement ) { // follow the average delta
					drawable.GlobalPosition = gripSources.Values.Average( x => x.TargetStartPosition + x.Position - x.StartPosition );
				}
			}
		}

		float computeScaleMultiplier () {
			if ( gripSources.Count <= 1 ) return 1;

			var avgStartPos = gripSources.Values.Average( a => a.StartPosition );
			var avgEndPos = gripSources.Values.Average( a => a.Position );
			var avgStartDist = gripSources.Values.Average( a => ( avgStartPos - a.StartPosition ).Length );
			var avgEndDist = gripSources.Values.Average( a => ( avgEndPos - a.Position ).Length );

			return avgEndDist / avgStartDist;
		}
	}

	public class GripManager {
		Dictionary<IGripable, GripGroup> activeGrips = new();

		public GripGroup? TryGrip ( Drawable3D target, Drawable3D source ) {
			if ( target is not IGripable gripable ) return null;

			GripGroup? group = activeGrips.GetValueOrDefault( gripable );
			if ( group is null ) {
				group = new();
				if ( group.TryGrip( target, source ) ) {
					activeGrips.Add( gripable, group );
					return group;
				}
				return null;
			}

			return group.TryGrip( target, source ) ? group : null;
		}

		public void Release ( Drawable3D target, Drawable3D source ) {
			if ( target is not IGripable gripable ) return;

			if ( !activeGrips.TryGetValue( gripable, out var group ) ) return;

			group.Release( source );
			if ( group.Target is null ) activeGrips.Remove( gripable );
		}

		public void Release ( Drawable3D source ) {
			foreach ( var i in activeGrips.Keys.ToArray() ) {
				Release( (Drawable3D)i, source );
			}
		}

		public void Update () {
			foreach ( var i in activeGrips.Values ) i.Update();
		}
	}
}