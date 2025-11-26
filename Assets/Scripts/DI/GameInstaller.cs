using Zenject;
using MiniIT.EVENTS;
using MiniIT.SCORE;
using MiniIT.LEVELS;
using MiniIT.BALL;
using MiniIT.LIVES;
using MiniIT.GAMEOVER;
using MiniIT.GAMESTATE;
using MiniIT.MOVEMENT;
using MiniIT.AUDIO;

namespace MiniIT.DI
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameEvents>().To<GameEvents>().AsSingle();
            Container.Bind<IScoreManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<IBall>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ILivesManager>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<LevelSelector>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ILevelCompletion>().FromComponentInHierarchy().AsSingle();

            Container.Bind<GameOverMenu>().FromComponentInHierarchy().AsSingle();

            Container.Bind<IGameStateManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<IPlatformMover>().FromComponentInHierarchy().AsSingle();

            Container.Bind<IAudioManager>().To<AudioManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}