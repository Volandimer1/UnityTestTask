using _Bludoku.Scripts.Analytics;
using _Bludoku.Scripts.Boards;
using _Bludoku.Scripts.Combo;
using _Bludoku.Scripts.Core;
using _Bludoku.Scripts.Score;
using _Bludoku.Scripts.UI;
using UnityEngine;

namespace _Bludoku.Scripts
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        [SerializeField] private ScoreMediator scoreMediator;
        [SerializeField] private UIMediator uiMediator;
        [SerializeField] private Board board;
        [SerializeField] private FiguresController figuresController;
        [SerializeField] private ComboFeedbackView comboFeedbackView;

        private IAnalyticsService _analyticsService;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            _analyticsService = new ConsoleAnalyticsService();

            var comboSystem = new ComboSystem();
            scoreMediator.Bind(comboSystem);
            comboFeedbackView.Bind(comboSystem);
            scoreMediator.RestoreProgress();

            figuresController.OnFigureMoveResolved += TrackFigureMove;
            scoreMediator.BoosterActivated += TrackBoosterActivation;
            figuresController.OnGameOver += HandleGameOver;

            board.LoadGrid();
            figuresController.LoadFigures();
        }

        public void NewGame()
        {
            BoardSaveLoad.Delete();
            board.ResetBoard();
            figuresController.ResetFigures();
            uiMediator.HideGameOver();
            scoreMediator.ResetScore();
        }

        public void SecondChance()
        {
            uiMediator.HideGameOver();
            figuresController.UpdateToEasyFigures();
            _analyticsService.Track(AnalyticsEvent.PowerUpUsed("second_chance"));
        }

        private void OnDestroy()
        {
            figuresController.OnFigureMoveResolved -= TrackFigureMove;
            scoreMediator.BoosterActivated -= TrackBoosterActivation;
            figuresController.OnGameOver -= HandleGameOver;
        }

        private void TrackFigureMove(int figureId, bool accepted)
        {
            _analyticsService.Track(AnalyticsEvent.PieceMoved(figureId, accepted));
        }

        private void TrackBoosterActivation(int comboCount)
        {
            _analyticsService.Track(AnalyticsEvent.BonusReceived("score_boost", comboCount));
        }

        private void HandleGameOver()
        {
            uiMediator.ShowGameOver();
        }
    }
}
