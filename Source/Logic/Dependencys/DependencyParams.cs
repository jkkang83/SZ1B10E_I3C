using S2System.Vision;

namespace FZ4P
{
    public class DependencyParams
    {
        public MILlib Cam { get; set; } = null;
        public FVision vision { get; set; } = null;
        public AK73XX driveIC { get; set; } = null;
        public LogText logText { get; set; } = null;

        public Global global { get; set; } = null;
    }
}
