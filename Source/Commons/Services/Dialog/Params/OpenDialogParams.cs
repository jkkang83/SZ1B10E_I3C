namespace FZ4P.Commons.Services
{
    public class OpenDialogParams
    {
        public string Title { get; set; }
        public string Filter { get; set; }
        public string InitialDirectory { get; set; }

        public IDialogResultFunc resultFunction;
    }
}
