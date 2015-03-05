﻿using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2C;
using Windows.Foundation;

namespace GHI.Athens.Gadgeteer.SocketInterfaces {
	public enum DigitalInputOutputMode {
		Input,
		Output
	}

	public delegate Task<DigitalInput> DigitalInputCreator(Socket socket, SocketPinNumber pinNumber, GpioInputDriveMode driveMode);
	public delegate Task<DigitalOutput> DigitalOutputCreator(Socket socket, SocketPinNumber pinNumber, bool initialValue);
	public delegate Task<DigitalInterrupt> DigitalInterruptCreator(Socket socket, SocketPinNumber pinNumber, GpioInterruptType interruptType, GpioInputDriveMode driveMode);
	public delegate Task<DigitalInputOutput> DigitalInputOutputCreator(Socket socket, SocketPinNumber pinNumber, DigitalInputOutputMode mode, GpioInputDriveMode driveMode, bool initialOutputValue);
	public delegate Task<AnalogInput> AnalogInputCreator(Socket socket, SocketPinNumber pinNumber);
	public delegate Task<AnalogOutput> AnalogOutputCreator(Socket socket, SocketPinNumber pinNumber, double initialValue);
	public delegate Task<PwmOutput> PwmOutputCreator(Socket socket, SocketPinNumber pinNumber);
	public delegate Task<I2CDevice> I2CDeviceCreator(Socket socket);
	public delegate Task<SpiDevice> SpiDeviceCreator(Socket socket);
	public delegate Task<SerialDevice> SerialDeviceCreator(Socket socket);
	public delegate Task<CanDevice> CanDeviceCreator(Socket socket);

	public abstract class DigitalOutput {
		public abstract void Write(bool value);
		public abstract bool Read();

		public bool Value {
			get {
				return this.Read();
			}
			set {
				this.Write(value);
			}
		}

		public void SetHigh() {
			this.Write(true);
		}

		public void SetLow() {
			this.Write(false);
		}
	}

	public abstract class DigitalInput {
		public abstract bool Read();

		public bool Value {
			get {
				return this.Read();
			}
		}

		public abstract GpioInputDriveMode DriveMode { get; set; }
	}

	public abstract class DigitalInterrupt : DigitalInput {
		public abstract GpioInterruptType InterruptType { get; set; }

		public event TypedEventHandler<DigitalInterrupt, GpioInterruptEventArgs> Interrupt;

		protected void OnInterrupt(GpioInterruptEventArgs e) {
			this.Interrupt?.Invoke(this, e);
		}
	}

	public abstract class DigitalInputOutput {
		public abstract void Write(bool value);
		public abstract bool Read();

		public bool Value {
			get {
				return this.Read();
			}
			set {
				this.Write(value);
			}
		}

		public DigitalInputOutputMode Mode { get; protected set; }

		public abstract GpioInputDriveMode DriveMode { get; set; }
	}

	public abstract class AnalogInput {
		public abstract double MaxVoltage { get; }

		public abstract double ReadVoltage();

		public double ReadProportion() {
			return this.ReadVoltage() / this.MaxVoltage;
		}

		public double Voltage {
			get {
				return this.ReadVoltage();
			}
		}

		public double Proportion {
			get {
				return this.ReadProportion();
			}
		}
	}

	public abstract class AnalogOutput {
		private double voltage = 0.0;

		public abstract double MaxVoltage { get; }

		public abstract void WriteVoltage(double voltage);

		public void WriteProportion(double value) {
			this.WriteVoltage(value / this.MaxVoltage);
		}

		public double Voltage {
			get {
				return this.voltage;
			}
			set {
				this.voltage = value;

				this.WriteVoltage(value);
			}
		}

		public double Proportion {
			get {
				return this.Voltage / this.MaxVoltage;
			}
			set {
				this.Voltage = this.MaxVoltage * value;
			}
		}
	}

	public abstract class PwmOutput {
		private bool enabled;
		private double frequency;
		private double dutyCycle;

		protected abstract void SetEnabled(bool state);
		protected abstract void SetValues(double frequency, double dutyCycle);

		public void Set(double frequency, double dutyCycle) {
			this.SetValues(frequency, dutyCycle);

			this.frequency = frequency;
			this.dutyCycle = dutyCycle;
		}

		public bool Enabled {
			get {
				return this.enabled;
			}
			set {
				this.SetEnabled(value);

				this.Enabled = value;
			}
		}

		public double Frequency {
			get {
				return this.frequency;
			}
			set {
				this.Set(value, this.dutyCycle);
			}
		}

		public double DutyCycle {
			get {
				return this.dutyCycle;
			}
			set {
				this.Set(this.frequency, value);
			}
		}
	}

	public abstract class I2CDevice {
		private byte[] write1;
		private byte[] write2;
		private byte[] read1;

		public abstract I2CTransferStatus Write(byte[] buffer, out uint transferred);
		public abstract I2CTransferStatus Read(byte[] buffer, out uint transferred);
		public abstract I2CTransferStatus WriteRead(byte[] writeBuffer, byte[] readBuffer, out uint transferred);

		protected I2CDevice() {
			this.write1 = new byte[1];
			this.write2 = new byte[2];
			this.read1 = new byte[1];
		}

		public I2CTransferStatus WriteRead(byte[] writeBuffer, byte[] readBuffer) {
			uint transferred;

			return this.WriteRead(writeBuffer, readBuffer, out transferred);
		}

		public I2CTransferStatus Write(byte[] buffer) {
			uint transferred;

			return this.Write(buffer, out transferred);
		}

		public I2CTransferStatus Read(byte[] buffer) {
			uint transferred;

			return this.Read(buffer, out transferred);
		}

		public I2CTransferStatus WriteRegister(byte register, byte value) {
			this.write2[0] = register;
			this.write2[1] = value;

			return this.Write(this.write2);
		}

		public byte ReadRegister(byte register) {
			this.write1[0] = register;

			this.WriteRead(this.write1, this.read1);

			return read1[0];
		}

		public I2CTransferStatus ReadRegister(byte register, out byte value) {
			this.write1[0] = register;

			var result = this.WriteRead(this.write1, this.read1);

			value = read1[0];

			return result;
		}

		public byte[] ReadRegisters(byte register, uint count) {
			var result = new byte[count];

			this.write1[0] = register;

			this.WriteRead(this.write1, result);

			return result;
		}

		public I2CTransferStatus ReadRegisters(byte register, byte[] values) {
			this.write1[0] = register;

			return this.WriteRead(this.write1, values);
		}
	}

	public abstract class SpiDevice {

	}

	public abstract class SerialDevice {

	}

	public abstract class CanDevice {

	}
}