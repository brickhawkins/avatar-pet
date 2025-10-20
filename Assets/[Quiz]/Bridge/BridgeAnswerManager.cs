using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
//using Lean.Pool;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Anteena.Quiz.Bridge
{
    public class BridgeAnswerManager : MonoBehaviour
    {
        /*
        [SerializeField] BridgeManager bridgeManager;
        [SerializeField] ChannelBridge channelBridge;
        [SerializeField] CanvasFader canvasFader;
        [SerializeField] LeanGameObjectPool pool;
        [SerializeField] Transform parent;
        [SerializeField] DropZoneUI[] dropZoneUIs;

        private readonly CancellationTokenSource cts = new();

        private void Start()
        {
            channelBridge.Current.Subscribe(value =>
            {
                if (value.Key != BridgeType.INIT_QUIZ)
                    return;

                LoadData(value.Value);
                canvasFader.Hide().Forget();

            }).AddTo(cts.Token);
        }

        public void LoadData(string json)
        {
            try
            {
                QuizData[] quizData = JsonConvert.DeserializeObject<QuizData[]>(json);
                Debug.Log("Data loaded: " + quizData.Length);
                Populate(quizData);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
        }

        public void Populate(QuizData[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                GameObject answerObject = pool.Spawn(parent);
                DragAnswerUI answerUI = answerObject.GetComponent<DragAnswerUI>();

                answerUI.SetData(data[i]);
            }
        }

        public void CheckOrder()
        {
            int[] correctOrder = { 1, 2, 3, 4, 5 };
            int[] placedOrder = dropZoneUIs
                .Select(zone => (zone.Current != null) ? zone.Current.Data.Order : -1)
                .ToArray();

            bool isCorrect = correctOrder.SequenceEqual(placedOrder);

            if (isCorrect)
            {
                Debug.Log("Correct Order!");
            }
            else
            {
                Debug.Log("Incorrect Order. Try Again.");
            }

            bridgeManager.CompleteQuiz(isCorrect);
        }
        */
    }
}