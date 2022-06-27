using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Default interface implementation (at least in unity)

1. To the outside objects only standard nonimplemented interfaces are flattned.

2. Internally for series of implemented interfaces they treat each other flattened.

3.

*/

public class CheckInterfaces : MonoBehaviour
{
    void Start()
    {
        Tester tester = new Tester();

        // * The only public, outside accessible methods through tester
        tester.MustHaveA();
        tester.ReuppTestAMethod();
        string must = tester.CORE_MUST_IMPLEMENT_PUBLIC;

        TestA testerAsA = tester;
        testerAsA.PublicCore();
        testerAsA.UnspecifiedCore();
        testerAsA.PleasePublicDefaultA(); // ui

        TestB testerAsB = tester;
        testerAsB.PublicCore();
        testerAsB.UnspecifiedCore();
        testerAsB.UnspecifiedB(); // ui
        testerAsB.PublicNeyB(); // ui
    }
}

public class Tester : TestA, TestB
{
    public static void Main()
    {
        // Creating an object
        Tester t = new Tester();

        // Calling method
        t.MustHaveA();
    }

    string text = "provided text";

    string TestA.GiveString
    {
        get => text;
        set => text = value;
    }
    public string CORE_MUST_IMPLEMENT_PUBLIC { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    string Core.YesYouMustToo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void MustHaveA()
    {
        Debug.Log($"Just a plain old necessary flat interface method required by TestA");
    }

    public void ReuppTestAMethod()
    {
        (this as TestA).PleasePublicDefaultA();
    }

    void TestA.MustHaveA()
    {
        throw new NotImplementedException();
    }
}

public interface TestA : Core
{
    void MustHaveA();

    // * THIS IS SUPER IMPORTANT
    protected string GiveString { get; set; }

    public void PleasePublicDefaultA()
    {
        Debug.Log(GiveString);
    }
}

// * Ok, an interface can notmally use default interface implementation, they are just flat, thats cool
public interface TestB : Core
{
    // * Unspecified remains public as it used to
    void UnspecifiedB()
    {
        Debug.Log("protecccc");
        UnspecifiedCore();
    }

    protected void ProtectedB()
    {
        Debug.Log("protecccc");
        ProtectedCore();
    }

    public void PublicNeyB()
    {
        Debug.Log("Public Ney B");
        PublicCore();
        CORE_MUST_IMPLEMENT_PUBLIC = "ay";
    }
}

public interface Core
{
    static string CORE_STRING = "it works";

    protected string GetCoreString => CORE_STRING;

    public string CORE_MUST_IMPLEMENT_PUBLIC { get; set; }

    protected string YesYouMustToo { get; set; }

    void UnspecifiedCore()
    {
        Debug.Log("Unspecified core called");
    }

    protected void ProtectedCore()
    {
        Debug.Log("Protected core called");
    }

    public void PublicCore()
    {
        Debug.Log("Public core called");
    }
}
