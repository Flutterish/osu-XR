﻿using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Game;
using osuTK;
using osuTK.Graphics.ES30;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using WindowState = osu.Framework.Platform.WindowState;

namespace osu.XR.GameHosts {
	public abstract class ExtendedRealityGameHost : GameHost {
        // TBD
        protected ExtendedRealityGameHost ( string gameName = "", ToolkitOptions toolkitOptions = null ) : base( gameName, toolkitOptions ) {

        }
	}
}
