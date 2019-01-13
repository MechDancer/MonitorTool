﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using MechDancer.Common;
using MonitorTool.Controls;
using static Windows.UI.Core.CoreDispatcherPriority;

namespace MonitorTool.Pages {
	public sealed partial class ProbePage {
		private static readonly HashSet<IPEndPoint> Memory
			= new HashSet<IPEndPoint>();

		private bool _running = true;

		public ProbePage() => InitializeComponent();

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			View.Dispatcher.RunAsync
				(Low, async () => {
						  foreach (var group in Memory) Add(group);
						  while (_running) {
							  await Task.Delay(500);
							  foreach (var view in View.Items?.OfType<ProbeView>()
												?? new ProbeView[] { })
								  view.Refresh(TimeSpan.FromSeconds(1));
						  }
					  });

			if (View.Items?.FirstOrDefault() is GroupSelector selector)
				selector.ButtonClick += group => Memory.Add(group).Then(() => Add(group));
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
			=> _running = false;

		private void Add(IPEndPoint group) {
			Debug.Assert(View.Items != null, "View.Items != null");
			var view = new ProbeView(group);
			view.CloseButtonClick += () => View.Items.Remove(view);
			view.CloseButtonClick += () => Memory.Remove(view.Group);
			View.Dispatcher.RunAsync(High, () => View.Items.Insert(View.Items.Count - 1, view));
		}
	}
}