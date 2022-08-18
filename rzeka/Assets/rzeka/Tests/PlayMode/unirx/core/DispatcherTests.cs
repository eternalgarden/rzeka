using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using Rzeka.Unirx;

public class DispatcherTests
{
    [UnityTest]
    public IEnumerator EveryUpdateObservableWorks()
    {
        long value = 0;

        var disp = UnityObservable
            .EveryUpdate()
            .Subscribe(onNext: x => value = x);

        yield return new WaitForSeconds(0.1f);

        Assert.Greater(value, 0);
    }


    [UnityTest]
    public IEnumerator DoesEveryUpdateTickCountAgreeWithFrames()
    {
        Application.targetFrameRate = 10;

        long value = 0;

        var disp = UnityObservable
            .EveryUpdate()
            .Subscribe(onNext: x => value = x);

        yield return new WaitForSeconds(1f);

        Assert.AreEqual(10, value);

        Application.targetFrameRate = -1;
    }
}