using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Configuration;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public partial class HandSkeletonPointer : CompositeDrawable3D, IPointerSource {
	OsuXrHandSkeleton skeleton;
	TouchPointer[] fingertips;
	Pointer[] fingertipPointers;

	List<Pointer> activePointers = new();

	public HandSkeletonPointer ( VrController controller ) {
		AddInternal( skeleton = new OsuXrHandSkeleton( (Controller)controller.Source, controller.Hand is Hand.Left ? VrAction.LeftHandSkeleton : VrAction.RightHandSkeleton ) );
		fingertips = new TouchPointer[] {
			new(), new(), new(), new(), new()
		};
		fingertipPointers = fingertips.Select( x => new Pointer(x, controller.InteractionSystem) ).ToArray();
		foreach ( var i in fingertips )
			i.RadiusBindable.Value = 0.012f;

		activeFingers.BindValueChanged( v => {
			foreach ( var i in fingertips ) {
				if ( i.Parent != null ) {
					RemoveInternal( i, disposeImmediately: false );
				}
			}

			activePointers.Clear();
			foreach ( var (i, flag) in new[] { (0, Fingers.Index), (1, Fingers.Middle), (2, Fingers.Ring), (3, Fingers.Pinky), (4, Fingers.Thumb) } ) {
				if ( v.NewValue.HasFlag( flag ) || (v.NewValue == Fingers.None && flag == Fingers.Index) ) {
					activePointers.Add( fingertipPointers[i] );
					AddInternal( fingertips[i] );
				}
			}
		}, true );
	}

	public IEnumerable<Pointer> Pointers => activePointers;

	public RentedArray<PointerHit?> UpdatePointers ( Vector3 playerPosition, Vector3 position, Quaternion rotation ) {
		var arr = MemoryPool<PointerHit?>.Shared.Rent( activePointers.Count );
		int index = 0;
		for ( int i = 0; i < 5; i++ ) {
			if ( activePointers.Contains( fingertipPointers[i] ) )
			arr[index++] = fingertipPointers[i].Update( playerPosition, skeleton.LocalMatrix.Apply( skeleton.Fingertips[i] ), Quaternion.Identity );
		}
		return arr;
	}

	public void SetTint ( Colour4 tint ) {
		skeleton.Tint = tint;
		foreach ( var i in fingertips ) {
			i.SetTint( tint );
		}
	}

	Bindable<Fingers> activeFingers = new( Fingers.All );
	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager? config ) {
		config?.BindWith( OsuXrSetting.HandSkeletonFingers, activeFingers );
	}
}

public partial class OsuXrHandSkeleton : BasicModel {
	HandSkeletonAction source = null!;
	Controller controller;
	Enum name;
	public OsuXrHandSkeleton ( Controller controller, Enum name ) {
		this.controller = controller;
		this.name = name;
	}

	[Resolved]
	VrCompositor compositor { get; set; } = null!;

	Bindable<MotionRange> motionRange = new();

	public readonly Vector3[] Fingertips = new Vector3[5];

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager? config ) {
		source = compositor.Input.GetAction<HandSkeletonAction>( name, controller );
		config?.BindWith( OsuXrSetting.HandSkeletonMotionRange, motionRange );
	}

	protected override void Update () {
		if ( source.FetchData( motionRange: motionRange.Value is MotionRange.WithController ? Valve.VR.EVRSkeletalMotionRange.WithController : Valve.VR.EVRSkeletalMotionRange.WithoutController ) != true ) {
			if ( Mesh.Indices.Any() ) {
				Mesh.Clear();
				Mesh.CreateFullUpload().Enqueue();
			}
			return;
		}

		Position = compositor.ActivePlayer?.InGlobalSpace( controller.Position ) ?? controller.Position;
		Rotation = compositor.ActivePlayer?.InGlobalSpace( controller.Rotation ) ?? controller.Rotation;
		var offset = Vector3.UnitY * 0.0007f;

		Mesh.Clear();
		for ( int i = 2 /*0 is root bone, 1 is wrist*/; i < source.BoneCount - 5 /*there are 5 aux bones*/; i++ ) {
			var bone = source.GetBoneData( i );
			var parent = source.GetBoneData( source.ParentBoneIndex( i ) );

			Mesh.AddQuad( new Quad3 {
				TL = parent.Position.XyzToOsuTk() + offset,
				TR = parent.Position.XyzToOsuTk() - offset,
				BL = bone.Position.XyzToOsuTk() + offset,
				BR = bone.Position.XyzToOsuTk() - offset
			} );
			Mesh.AddCircle( bone.Position.XyzToOsuTk(), bone.Rotation.ToOsuTk().Apply( Vector3.UnitY ), bone.Rotation.ToOsuTk().Apply( Vector3.UnitX * 0.003f ), 32 );
		}
		Mesh.CreateFullUpload().Enqueue();

		Fingertips[0] = source.GetBoneData( (int)HandSkeletonBone.IndexFinger4 ).Position.ToOsuTk().Xyz;
		Fingertips[1] = source.GetBoneData( (int)HandSkeletonBone.MiddleFinger4 ).Position.ToOsuTk().Xyz;
		Fingertips[2] = source.GetBoneData( (int)HandSkeletonBone.RingFinger4 ).Position.ToOsuTk().Xyz;
		Fingertips[3] = source.GetBoneData( (int)HandSkeletonBone.PinkyFinger4 ).Position.ToOsuTk().Xyz;
		Fingertips[4] = source.GetBoneData( (int)HandSkeletonBone.Thumb3 ).Position.ToOsuTk().Xyz;
	}

	enum HandSkeletonBone : int {
		Root,
		Wrist,
		Thumb0,
		Thumb1,
		Thumb2,
		Thumb3,
		IndexFinger0,
		IndexFinger1,
		IndexFinger2,
		IndexFinger3,
		IndexFinger4,
		MiddleFinger0,
		MiddleFinger1,
		MiddleFinger2,
		MiddleFinger3,
		MiddleFinger4,
		RingFinger0,
		RingFinger1,
		RingFinger2,
		RingFinger3,
		RingFinger4,
		PinkyFinger0,
		PinkyFinger1,
		PinkyFinger2,
		PinkyFinger3,
		PinkyFinger4,
		AuxThumb,
		AuxIndexFinger,
		AuxMiddleFinger,
		AuxRingFinger,
		AuxPinkyFinger
	};
}