using ICities;

namespace RemoveCows
{
    public class Loader : LoadingExtensionBase
    {
        Helper _helper;

        public override void OnCreated(ILoading loading)
        {
            _helper = Helper.Instance;

            _helper.GameLoaded = loading.loadingComplete;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
                _helper.GameLoaded = true;
        }

        public override void OnLevelUnloading()
        {
            _helper.GameLoaded = false;
        }
    }
}