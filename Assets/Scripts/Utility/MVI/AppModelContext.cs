namespace Assets.Scripts.Utility.MVI
{
    public sealed class AppModelContext : IModelContext
    {
        public IRegistry<MeshData, MeshRegistry> Meshes { get; }
        public IRegistry<Parameter, ParameterRegistry> Parameters { get; }
        public IParamCurveRegistry ParamCurves { get; }
        public IRegistry<ParamPoint, ParamPointRegistry> ParamPoints { get; }
        public IKeyFrameRegistry KeyFrames { get; }

        public IGeneralSettings GeneralSettings { get; }
        public ISessionInfo SessionInfo { get; }


        public AppModelContext(
            IRegistry<MeshData, MeshRegistry> meshes,
            IRegistry<Parameter, ParameterRegistry> parameters,
            IParamCurveRegistry paramCurves,
            IRegistry<ParamPoint, ParamPointRegistry> paramPoints,
            IKeyFrameRegistry keyFrames,
            IGeneralSettings generalSettings,
            ISessionInfo sessionInfo
        )
        {
            Meshes = meshes;
            Parameters = parameters;
            ParamCurves = paramCurves;
            ParamPoints = paramPoints;
            KeyFrames = keyFrames;
            GeneralSettings = generalSettings;
            SessionInfo = sessionInfo;
        }
    }
}