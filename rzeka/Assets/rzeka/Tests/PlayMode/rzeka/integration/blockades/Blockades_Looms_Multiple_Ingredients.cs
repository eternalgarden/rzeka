
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Blockades_Looms_Multiple_Ingredients : IntegrationBase
    {
        [UnityTest]
        public IEnumerator a_IsANumberAndAName_Properly_Blocked()
        {
            // -------------
            
            bool blockedByNumber = false;
            bool blockedByName = false;

            using var x = Rzeka.Loom<ANumber,AName,ANumberAndName>(
                who: this,
                spell: o => o
                    .Select(glyph => new ANumberAndName(glyph.one.Number, glyph.two.Name))
            );

            yield return null;

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<ANumber>(out TBindingSpell[] blockedbynumber))
            // {
            //     blockedByNumber = blockedbynumber.Length == 1;
            // }

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<AName>(out TBindingSpell[] blockedbyname))
            // {
            //     blockedByName = blockedbyname.Length == 1;
            // }

            AssertEqual((true, true), (blockedByNumber, blockedByName));

            // -------------
        }

        [UnityTest]
        public IEnumerator b_IsANumberAndAName_Blocked_NotConjurable()
        {
            // -------------
            
            using var x1 = Rzeka.Loom<ANumber,AName,ANumberAndName>(
                who: this,
                spell: o => o
                    .Select(glyph => new ANumberAndName(glyph.one.Number, glyph.two.Name))
            );

            yield return null;

            // AssertEqual(false, Rzeka.TheLibrary.IsConjurable<ANumberAndName>());

            // -------------
        }

        [UnityTest]
        public IEnumerator c_IsANumberAndAName_Properly_Blocked_ByANumber()
        {
            // -------------
            
            bool blockedByNumber = false;
            bool blockedByName = false;

            using var x1 = Pluck_AName("Zosia");

            using var x = Rzeka.Loom<ANumber,AName,ANumberAndName>(
                who: this,
                spell: o => o
                    .Select(glyph => new ANumberAndName(glyph.one.Number, glyph.two.Name))
            );

            yield return null;

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<ANumber>(out TBindingSpell[] blockedbynumber))
            // {
            //     blockedByNumber = blockedbynumber.Length == 1;
            // }

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<AName>(out TBindingSpell[] blockedbyname))
            // {
            //     blockedByName = blockedbyname.Length == 1;
            // }

            AssertEqual((true, false), (blockedByNumber, blockedByName));

            // -------------
        }

        [UnityTest]
        public IEnumerator d_IsANumberAndAName_Properly_UnBlocked_By_ANumber()
        {
            // -------------
            
            bool blockedByNumber = false;
            bool blockedByName = false;

            using var x1 = Pluck_AName("Alicja");

            using var x = Rzeka.Loom<ANumber,AName,ANumberAndName>(
                who: this,
                spell: o => o
                    .Select(glyph => new ANumberAndName(glyph.one.Number, glyph.two.Name))
            );

            using var x2 = Pluck_ANumber(1);

            yield return null;

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<ANumber>(out TBindingSpell[] blockedbynumber))
            // {
            //     blockedByNumber = blockedbynumber.Length == 1;
            // }

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<AName>(out TBindingSpell[] blockedbyname))
            // {
            //     blockedByName = blockedbyname.Length == 1;
            // }

            AssertEqual((false, false), (blockedByNumber, blockedByName));

            // -------------
        }
        
        [UnityTest]
        public IEnumerator e_IsANumberAndAName_Properly_UnBlocked_ByANumber()
        {
            // -------------
            
            bool blockedByNumber = false;
            bool blockedByName = false;

            using var x = Rzeka.Loom<ANumber,AName,ANumberAndName>(
                who: this,
                spell: o => o
                    .Select(glyph => new ANumberAndName(glyph.one.Number, glyph.two.Name))
            );

            using var x2 = Pluck_ANumber(1);

            yield return null;

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<ANumber>(out TBindingSpell[] blockedbynumber))
            // {
            //     blockedByNumber = blockedbynumber.Length == 1;
            // }

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<AName>(out TBindingSpell[] blockedbyname))
            // {
            //     blockedByName = blockedbyname.Length == 1;
            // }

            AssertEqual((false, true), (blockedByNumber, blockedByName));

            // -------------
        }

        [UnityTest]
        public IEnumerator f_IsANumberAndAName_Properly_UnBlocked_ByANumber_And_AName()
        {
            // -------------
            
            bool blockedByNumber = false;
            bool blockedByName = false;

            using var x = Rzeka.Loom<ANumber,AName,ANumberAndName>(
                who: this,
                spell: o => o
                    .Select(glyph => new ANumberAndName(glyph.one.Number, glyph.two.Name))
            );

            using var x2 = Pluck_ANumber(1);
            using var x1 = Pluck_AName("Alicja");

            yield return null;

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<ANumber>(out TBindingSpell[] blockedbynumber))
            // {
            //     blockedByNumber = blockedbynumber.Length == 1;
            // }

            // if (Rzeka.TheLibrary.IsTypeBlockingSpells<AName>(out TBindingSpell[] blockedbyname))
            // {
            //     blockedByName = blockedbyname.Length == 1;
            // }

            AssertEqual((false, false), (blockedByNumber, blockedByName));

            // -------------
        }
    }
}