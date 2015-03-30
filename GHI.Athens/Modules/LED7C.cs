﻿using GHI.Athens.Gadgeteer;
using GHI.Athens.Gadgeteer.SocketInterfaces;
using System.Threading.Tasks;

namespace GHI.Athens.Modules {
	public class LED7C : Module {
		public override string Name { get; } = "LED7C";
		public override string Manufacturer { get; } = "GHI Electronics, LLC";

		private DigitalIO red;
		private DigitalIO green;
		private DigitalIO blue;

		public enum Color {
			Off,
			Blue,
			Green,
			Cyan,
			Red,
			Magenta,
			Yellow,
			White
		}

		protected async override Task Initialize(Socket parentSocket) {
			this.red = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Four, false);
			this.green = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Five, false);
			this.blue = await parentSocket.CreateDigitalIOAsync(SocketPinNumber.Three, false);
		}
		public void SetColor(Color color) {
			int c = (int)color;

			this.red.Write((c & 4) != 0);
			this.green.Write((c & 2) != 0);
			this.blue.Write((c & 1) != 0);
		}
	}
}