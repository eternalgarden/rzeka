/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Matter.Default
{
    public class Matter_Default_03_Overloads
    {
        // -------------
        
        ITestableRzeka _rzeka;
        TestTools _tools;
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            _rzeka = new SpringRiver();
            _tools = new TestTools(_rzeka);

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------

            _rzeka.Dispose();

            yield return null;

            // -------------
        }
        
        [Test]
        [TestCase("Sally", 7)]
        [TestCase("Dipsy", 88)]
        public void a1_loom_2_shaped_receiveable_matter_single(string name, int favnum)
        {
            // -------------

            string zodiac = "Aries";

            using var loom = _rzeka.Loom<AName, ANumber, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.One.Name, zodiac, glyph.Two.Number);
                    }));
            
            UserData receivedData = default;

            using var d1 = _tools.Weave_UserData(m => receivedData = m);

            using var s1 = _tools.Strand_AName_Synchronous(name);
            using var s2 = _tools.Strand_ANumber_Synchronous(favnum);

            TestTools.AssertEqual(new UserData(name, zodiac, favnum), receivedData, UserData.NameZodiacFavNumberComparer);

            // -------------
        }
        
        
        
        [Test]
        [TestCase("Sally", "Aquarius", 5)]
        [TestCase("Dipsy", "Aries", 666)]
        public void b1_loom_3_shaped_receiveable_matter_single(string name, string zodiac, int favnum)
        {
            // -------------

            using var loom = _rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ANumber, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.One.Text, glyph.Two.Text, glyph.Three.Number);
                    }));
            
            UserData receivedData = default;

            using var d1 = _tools.Weave_UserData(m => receivedData = m);
            
            using var s1 = _tools.Strand_ArbitraryMatter1(name);
            using var s2 = _tools.Strand_ArbitraryMatter2(zodiac);
            using var s3 = _tools.Strand_ANumber_Synchronous(favnum);

            TestTools.AssertEqual(new UserData(name, zodiac, favnum), receivedData, UserData.NameZodiacFavNumberComparer);

            // -------------
        }
        
        /*
         *  THE TWO _multiple TESTS BELOW ARE QUITE IMPORTANT
         *  EVEN THOUGH THEY WERE TRYING TO PROVE THE INCORRECT THING
         *
         *  Originally the assertion for both looked opposite to what is now:
         *  Assert.AreEqual(false, areEqual, comment);
         *
         *  So both tests dont make much sense now since they dont prove anything.
         *
         *  Well, actually they do, I was surprised to see the final 'data' composed
         *  of only 'Buffy' + number names, but it does make sense since internally
         *  ReplaySubject<1> is currently being used by default.
         */
        
        [Test]
        [TestCase(
            new string[] { "Willow", "Xander", "Buffy"} , 
            new int[] { 6, 69, 7 })]
        public void a2_loom_2_shaped_receiveable_matter_multiple(string[] names, int[] favnums)
        {
            // -------------

            string tempzodiac = "whatevs";
            List<UserData> expectedData = new();

            for (var i = 0; i < names.Length; i++)
            {
                expectedData.Add(new UserData(names[i], tempzodiac, favnums[i]));
            }

            using var loom = _rzeka.Loom<ArbitraryMatter1, ANumber, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph => new UserData(
                        glyph.One.Text, 
                        tempzodiac, 
                        glyph.Two.Number)));
            
            List<UserData> receivedData = new();

            using var d1 = _tools.Weave_UserData(m => receivedData.Add(m));

            using var s1 = _tools.Strand_ArbitraryMatter1(names);
            using var s3 = _tools.Strand_ANumber_Synchronous(favnums);

            bool areEqual = expectedData.SequenceEqual(receivedData, UserData.NameZodiacFavNumberComparer);

            string comment = $"EXPECTED: {expectedData.Aggregate("", (s, data) => $"{s} {data.Name+data.Zodiac+data.FavNumber},")}\n" +
                             $"RECEIVED: {receivedData.Aggregate("", (s, data) => $"{s} {data.Name+data.Zodiac+data.FavNumber},")}";
            
            Assert.AreEqual(false, areEqual, comment);

            // -------------
        }
        
        [Test]
        [TestCase(
            new string[] { "Willow", "Xander", "Buffy"} , 
            new string[] { "Aquarius", "Scorpio", "Cancer"}, 
            new int[] { 6, 69, 7 })]
        public void b2_loom_3_shaped_receiveable_matter_multiple(string[] names, string[] zodiacs, int[] favnums)
        {
            // -------------

            List<UserData> expectedData = new();

            // Debug.Log(names.Length);

            for (int i = 0; i < names.Length; i++)
            {
                expectedData.Add(new UserData(names[i], zodiacs[i], favnums[i]));
            }

            using var loom = _rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ANumber, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph => new UserData(
                        glyph.One.Text, 
                        glyph.Two.Text, 
                        glyph.Three.Number)));
            
            List<UserData> receivedData = new();

            using var d1 = _tools.Weave_UserData(m => receivedData.Add(m));

            using var s1 = _tools.Strand_ArbitraryMatter1(names);
            using var s2 = _tools.Strand_ArbitraryMatter2(zodiacs);
            using var s3 = _tools.Strand_ANumber_Synchronous(favnums);

            bool areEqual = expectedData.SequenceEqual(receivedData, UserData.NameZodiacFavNumberComparer);

            string comment = $"EXPECTED: {expectedData.Aggregate("", (s, data) => $"{s} {data.Name+data.Zodiac+data.FavNumber},")}\n" +
                             $"RECEIVED: {receivedData.Aggregate("", (s, data) => $"{s} {data.Name+data.Zodiac+data.FavNumber},")}";
            
            Assert.AreEqual(false, areEqual, comment);

            // -------------
        }
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 28 November 2022 🌊 */