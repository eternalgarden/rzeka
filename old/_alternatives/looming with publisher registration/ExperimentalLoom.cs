using System;
using System.Reactive.Linq;
using UnityEngine;
using Rzeka.Unirx;
using Observable = System.Reactive.Linq.Observable; // TODO remove observable implementation from unirx
using TMPro;
using UnityEngine.UI;
using Rzeka;

namespace Looming
{
    public struct NewUserMessage : IMatter
    {
        public string text;

        // TODO Description property could be added to Matter instances automaticually
        // TODO without inheritance, then they could be structs and gain immutability
        public NewUserMessage(string text)
        {
            this.text = text;
        }
    }

    public struct CurrentUserMessageColor : IMatter
    {
        public Color color;
        public CurrentUserMessageColor(Color color)
        {
            this.color = color;
        }
    }

    public struct LastBlueishMessage : IMatter
    {
        public NewUserMessage message;
        public CurrentUserMessageColor color;

        public LastBlueishMessage(NewUserMessage message, CurrentUserMessageColor color)
        {
            this.message = message;
            this.color = color;
        }
    }


    public struct RequestDataForIndexRange : IMatter
    {
        public int from;
        public int to;

        public RequestDataForIndexRange(int from, int to)
        {
            this.from = from;
            this.to = to;
        }
    }

    // public struct ProvideDataForThoseIndices : IMatter
    // {
    //     public string[] stringsForIndices;
    //
    //      throw new NotImplementedException();
    // }

    public struct RequestDataForIndexRangeV2 : IMatter, IQuestion<RequestDataForIndexRangeV2.Answer>
    {
        public int from;
        public int to;

        public struct Answer : IAnswer
        {
            public RequestDataForIndexRangeV2 request;
            public string[] stringsForIndices;
            public object QuestionSource => request;

            public Answer(RequestDataForIndexRangeV2 request, string[] stringsForIndices)
            {
                this.request = request;
                this.stringsForIndices = stringsForIndices;
            }

        }
    }


    public class ExperimentalLoom : LoomingMono
    {
        [SerializeField] LoomingRzeka Rzeka;
        [SerializeField] TMP_InputField _inputField;
        [SerializeField] GameObject _textPrefab;
        [SerializeField] Transform _chatBoxContainer;
        [SerializeField] Button _toggleColourButton;
        [SerializeField] Image _colorPreview;
        [SerializeField] Transform _lastReddishMessage;

        EventHandler<string> newMessage;
        EventHandler toggleColorButtonClicked;


        readonly ThoughtFactory factory = new();

        [SerializeField] InspectableReactiveProperty<Color> _textColor;

        protected override void OnEnable()
        {
            // -------------

            base.OnEnable();

            // Register Return press
            q += UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Return))
                .Subscribe(_ =>
                {
                    string message = _inputField.text;
                    _inputField.text = "";

                    newMessage.Invoke(this, message);
                });

            _toggleColourButton.onClick.AddListener(() => toggleColorButtonClicked.Invoke(this, null));

            _toggleColourButton.onClick.AddListener(() =>
            {
                _textColor.Value = new Color(
                    r: UnityEngine.Random.Range(0.0f, 1.0f),
                    g: UnityEngine.Random.Range(0.0f, 1.0f),
                    b: UnityEngine.Random.Range(0.0f, 1.0f));
            });

            _textColor.Subscribe(c =>
            {
                _colorPreview.color = c;
            });


            SimpleTest1();

            //QATest1();

            // -------------
        }

        private void SimpleTest1()
        {
            Rzeka
                .Cast<CurrentUserMessageColor>(this, spell: () => Observable
                    .FromEventPattern(
                        h => toggleColorButtonClicked += h,
                        h => toggleColorButtonClicked -= h)
                    .Select(message =>
                    {
                        return new CurrentUserMessageColor(new Color(
                        r: UnityEngine.Random.Range(0.0f, 1.0f),
                        g: UnityEngine.Random.Range(0.0f, 1.0f),
                        b: UnityEngine.Random.Range(0.0f, 1.0f)));
                    }));

            Rzeka
                .Cast<NewUserMessage>(this, spell: () => Observable
                    .FromEventPattern<string>(
                        h => newMessage += h,
                        h => newMessage -= h)
                    .Select(message =>
                    {
                        return new NewUserMessage(message.EventArgs);
                    }));

            Rzeka
                .Cast<
                CurrentUserMessageColor, // ! what if it's provider unregisters beofre this combination stops working/unregisters itself
                NewUserMessage,
                LastBlueishMessage>(this, spell: pattern =>
                {
                    var plan = pattern.Then((currentCol, newMsg) => new { currentCol, newMsg });

                    return Observable
                        .When(plan)
                        .DistinctUntilChanged(comb => comb.newMsg.text)
                        .Where(comb =>
                        {
                            Debug.Log("are we here !");

                            Color col = comb.currentCol.color;
                            Debug.Log(col);

                            return col.r < 0.3f && col.b > col.g;
                        })
                        .Select(x =>
                        {
                            Debug.Log("bluish!");

                            return new LastBlueishMessage(
                                message: x.newMsg,
                                color: x.currentCol);
                        });
                });

            q += Rzeka
                .Weave<LastBlueishMessage>(this, spell: lastBlueishMessage =>
                {
                    return lastBlueishMessage
                        .Subscribe(o =>
                        {
                            Debug.Log("blueish");
                            GameObject child = _lastReddishMessage.childCount > 0
                                ? _lastReddishMessage.GetChild(0).gameObject
                                : null;

                            if (child is not null) Destroy(child);

                            var go = Instantiate(_textPrefab);
                            go.transform.SetParent(_lastReddishMessage);
                            go.GetComponentInChildren<TextMeshProUGUI>().text = o.message.text;
                            go.GetComponentInChildren<TextMeshProUGUI>().color = o.color.color;
                        });
                });

            q += Rzeka
                .Weave<NewUserMessage>(this, spell: newUserMessageMatter =>
                {
                    return newUserMessageMatter
                        .CombineLatest(second: _textColor)
                        .DistinctUntilChanged(o => o.First.text)
                        .Subscribe(o =>
                        {
                            var go = Instantiate(_textPrefab);
                            go.transform.SetParent(_chatBoxContainer);
                            go.GetComponentInChildren<TextMeshProUGUI>().text = o.First.text;
                            go.GetComponentInChildren<TextMeshProUGUI>().color = o.Second;
                        });
                });
        }

        private void QATest1()
        {
            string[] stringDataArray =
                        {
                "asdfasdgasdfgas",
                "asgasdrg",
                "234627452",
                "hjhf",
                "kjkjkujkjkjk",
                "1111111111111111111",
                "asdfasd",
                "asdfasdgsssssssssssssssssssssssssssssssssasdfgas",
                "czxcvvvvvvvv",
                "ajnhjjjjjjjjjjjjjjjjj",
                "asdlkfjjhbawneurfhakweuhtrbjUKASEHLRFJAWES",
                "11RDDD1D1D1D1DD1D1D1D1",
                "ASDFASDF",
                "575634976756",
                "512U4IU1Y2MJFJDGBFH",
                "ASDFASDFVABGAXFBNCXCLVKSDJFHBASJKEF",
                "uiiiiiiiiii",
                "ASDFASDK582y13u4rj",
                "asdfliajsedkjfuh",
                "cc",
                "v",
            };

            Rzeka.Answer<
                RequestDataForIndexRangeV2,
                RequestDataForIndexRangeV2.Answer>
                (this, onQuestion: question =>
                {
                    return question
                        .Select(q =>
                        {
                            int length = q.to - q.from;

                            var results = new string[length];

                            if (stringDataArray.Length <= q.to)
                            {
                                throw new Exception("Array out of bounds.");
                            };

                            Array.Copy(stringDataArray, q.from, results, 0, length);

                            return new RequestDataForIndexRangeV2.Answer(q, results);
                        });
                });

            Rzeka.Ask<
                RequestDataForIndexRangeV2,
                RequestDataForIndexRangeV2.Answer>
                (this,
                question: () => Observable
                    .Return(new RequestDataForIndexRangeV2() { from = 2, to = 5 }),
                onAnswer: answer => answer
                    .Subscribe(next =>
                    {
                        foreach (var x in next.stringsForIndices)
                        {
                            Debug.Log(x);
                        }
                    })
                );

            //_rzeka.Loom<RequestDataForIndexRange, ProvideDataForThoseIndices>(
            //    this,
            //    question => Observable.Create<ProvideDataForThoseIndices>(observer =>
            //    {
            //        int length = question.to - question.from;

            //        var results = new string[length];

            //        if (stringDataArray.Length <= question.to)
            //        {
            //            observer.OnError(new Exception("Array out of bounds."));
            //        };

            //        Array.Copy(stringDataArray, question.from, results, 0, length);

            //        return Disposable.Empty;
            //    }));

            //using var printer = _rzeka
            //    .Weave<RequestDataForIndexRange, ProvideDataForThoseIndices>(this, new(1, 3))
            //    .Subscribe(onNext: next =>
            //    {
            //        foreach (string s in next.stringsForIndices)
            //        {
            //            Debug.Log(s);
            //        }
            //    });
        }
    }
}
