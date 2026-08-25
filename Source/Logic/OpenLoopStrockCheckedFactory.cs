namespace FZ4P
{
    public class OpenLoopStrockCheckedFactory
    {
        public IOpenLoopExecutor CreateStrock(bool LogicCreate, DependencyParams param)
        {
            if (LogicCreate)
                return new OpenLoopStrockCheckImageSave(param.Cam, param.vision,param.driveIC, param.logText,param.global);
            else
                return new OpenLoopStrockCheck(param.Cam, param.vision, param.driveIC, param.logText, param.global);
        }
    }
}
