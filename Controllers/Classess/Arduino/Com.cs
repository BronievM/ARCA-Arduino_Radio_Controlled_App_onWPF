namespace ARCA_WPF_F.Controllers.Classess.Arduino
{
    public class Com
    {
        private string PortID;
        private string PortName;

        public Com(string portID, string portName)
        {
            PortID = portID;
            PortName = portName;
        }

        public Com() { }

        public string GetName()
        {
            return PortName;
        }

        public string GetId()
        {
            return PortID;
        }
    }
}
