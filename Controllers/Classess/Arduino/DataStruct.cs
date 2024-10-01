namespace ARCA_WPF_F.Controllers.Classess.Arduino
{
    public class DataStruct
    {
        public byte steer { get; set; }
        public byte accelerate { get; set; }
        public byte brake { get; set; }
        public bool F1 { get; set; }
        public bool F2 { get; set; }

        public DataStruct()
        {
            steer = 127;
            accelerate = 0;
            brake = 0;
            F1 = false;
            F2 = false;
        }

        public void SetData(byte steer, byte accelerate, byte brake, bool F1, bool F2)
        {
            this.steer = steer;
            this.accelerate = accelerate;
            this.brake = brake;
            this.F1 = F1;
            this.F2 = F2;
        }

        public void ResetData()
        {
            steer = 127;
            accelerate = 0;
            brake = 0;
            F1 = false;
            F2 = false;
        }

        public DataStruct GetData()
        {
            return this;
        }

        public void SetSteer(byte steer)
        {
            this.steer = steer;
        }

        public void SetAccelerator(byte accelerator)
        {
            this.accelerate = accelerator;
        }

        public void SetBrake(byte brake)
        {
            this.brake=brake;
        }

        public void SetF1(bool f1)
        {
            this.F1 = f1;
        }

        public void SetF2(bool f2)
        {
            this.F2 = f2;
        }

    }
}
