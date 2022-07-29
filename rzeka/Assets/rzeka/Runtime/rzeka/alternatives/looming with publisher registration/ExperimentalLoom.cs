using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine;
using Rzeka.Unirx;
using Observable = System.Reactive.Linq.Observable; // TODO remove observable implementation from unirx
using TMPro;
using UnityEngine.UI;
using System.Threading;
using System.Reactive.Subjects;
using Rzeka;

namespace Looming
{
    public struct NewUserMessageMatter : IMatter
    {
        public string text;

        // TODO Description property could be added to Matter instances automaticually
        // TODO without inheritance, then they could be structs and gain immutability
        public string Description => throw new NotImplementedException();

        public NewUserMessageMatter(string text)
        {
            this.text = text;
        }
    }

    public struct CurrentUserMessageColor : IMatter
    {
        public Color color;
        public string Description => throw new NotImplementedException();

        public CurrentUserMessageColor(Color color)
        {
            this.color = color;
        }
    }

    public struct LastReddishUserMessage : IMatter
    {
        public NewUserMessageMatter message;
        public CurrentUserMessageColor color;

        public string Description => throw new NotImplementedException();

        public LastReddishUserMessage(NewUserMessageMatter message, CurrentUserMessageColor color)
        {
            this.message = message;
            this.color = color;
        }
    }

    public struct InterestingIndicesMatter : IMatter
    {
        public int from;
        public int to;

        public InterestingIndicesMatter(int from, int to)
        {
            this.from = from;
            this.to = to;
        }

        public string Description => throw new NotImplementedException();
    }

    public struct GiverOfDataForThoseIndices : IMatter
    {
        public string[] stringsForIndices;

        public string Description => throw new NotImplementedException();
    }

    public class ExperimentalLoom : LoomingMono
    {
        [SerializeField] LoomingRzeka _rzeka;
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

            //QATest1();

            SimpleTest1();


            // -------------
        }

        private void SimpleTest1()
        {
            _rzeka
                .Loom<CurrentUserMessageColor>(this, Observable
                .FromEventPattern(
                    h => toggleColorButtonClicked += h,
                    h => toggleColorButtonClicked -= h)
                .Select(message =>
                {
                    Debug.Log("current col");

                    return new CurrentUserMessageColor(new Color(
                    r: UnityEngine.Random.Range(0.0f, 1.0f),
                    g: UnityEngine.Random.Range(0.0f, 1.0f),
                    b: UnityEngine.Random.Range(0.0f, 1.0f)));
                }));


            _rzeka
                .Loom<NewUserMessageMatter>(this, Observable
                .FromEventPattern<string>(
                    h => newMessage += h,
                    h => newMessage -= h)
                .Select(message =>
                {
                    return new NewUserMessageMatter(message.EventArgs);
                }));

            _rzeka
                .Loom<LastReddishUserMessage>(this,
                    _rzeka.Weave<NewUserMessageMatter>(this)
                    .CombineLatest(_rzeka.Weave<CurrentUserMessageColor>(this), 
                        resultSelector: (message, color) => new { message, color })
                    .DistinctUntilChanged(comb => comb.message)
                    .Where(comb =>
                    {
                        Color col = comb.color.color;
                        Debug.Log(col);

                        return col.r > 0.6f && col.b < 0.3f && col.g < 0.3f;
                    })
                    .Select(comb =>
                    {
                        Debug.Log("meow");
                        return new LastReddishUserMessage(comb.message, comb.color);
                    }));

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

            q += _rzeka
                .Weave<NewUserMessageMatter>(this)
                .CombineLatest(second: _textColor)
                .DistinctUntilChanged(o => o.First.text)
                .Subscribe(o =>
                {
                    var go = Instantiate(_textPrefab);
                    go.transform.SetParent(_chatBoxContainer);
                    go.GetComponentInChildren<TextMeshProUGUI>().text = o.First.text;
                    go.GetComponentInChildren<TextMeshProUGUI>().color = o.Second;
                });

            q += _rzeka
                .Weave<LastReddishUserMessage>(this)
                .Subscribe(o =>
                {
                    Debug.Log("reddish");
                    GameObject child = _lastReddishMessage.childCount > 0
                        ? _lastReddishMessage.GetChild(0).gameObject
                        : null;

                    if(child is not null) Destroy(child);

                    var go = Instantiate(_textPrefab);
                    go.transform.SetParent(_lastReddishMessage);
                    go.GetComponentInChildren<TextMeshProUGUI>().text = o.message.text;
                    go.GetComponentInChildren<TextMeshProUGUI>().color = o.color.color;
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

            _rzeka.Loom<InterestingIndicesMatter, GiverOfDataForThoseIndices>(
                this,
                question => Observable.Create<GiverOfDataForThoseIndices>(observer =>
                {
                    int length = question.to - question.from;

                    var results = new string[length];

                    if (stringDataArray.Length <= question.to)
                    {
                        observer.OnError(new Exception("Array out of bounds."));
                    };

                    Array.Copy(stringDataArray, question.from, results, 0, length);

                    return Disposable.Empty;
                }));

            using var printer = _rzeka
                .Weave<InterestingIndicesMatter, GiverOfDataForThoseIndices>(this, new(1, 3))
                .Subscribe(onNext: next =>
                {
                    foreach (string s in next.stringsForIndices)
                    {
                        Debug.Log(s);
                    }
                });
        }
    }
}
