import { Observable, filter, map } from "rxjs"
import { Who } from "../common/Who"
import { ArchivedSpell } from "../common/spells/ArchivedSpell"
import { ArchivedSpellOccurence } from "../occurences-archived/ArchivedSpellOccurence"
import { IArchivedOccurence } from "../occurences-archived/IArchivedOccurence"
import { SpellOccurenceCategory } from "../occurence-categories/SpellOccurenceCategory"
import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IRawOccurence } from "../occurences-raw/IRawOccurence"
import { IRawOtherSpellOccurence } from "../occurences-raw/spell/IRawOtherSpellOccurence"
import { IRawCreatedSpellOccurence } from "../occurences-raw/spell/IRawCreatedSpellOccurence"
import { IRawSpellOccurence } from "../occurences-raw/spell/IRawSpellOccurence"
import { ISerlializableBindingSpell } from "../common/spells/ISerlializableBindingSpell"

export class SpellArchive {
    private activeSpells: Map<string, ArchivedSpell> = new Map<
        string,
        ArchivedSpell
    >()

    private forgottenSpells: Map<string, ArchivedSpell> = new Map<
        string,
        ArchivedSpell
    >()

    newSpellOccurenceProcessed: Observable<IArchivedOccurence>

    requested: string[] = []

    constructor(newOccurenceObservable: Observable<IRawOccurence>) {
        this.newSpellOccurenceProcessed = newOccurenceObservable.pipe(
            filter(x => x.occurenceCategory == OccurenceCategory.Spell),
            map(x => {
                return x as IRawSpellOccurence
            }),
            map(occ => {
                const category = occ.spellOccurenceCategory

                let knownSpell: ArchivedSpell | undefined

                if (category == SpellOccurenceCategory.Created) {
                    knownSpell = this.processNewCreatedSpellOccurence(occ)
                } else {
                    knownSpell = this.processOtherSpellOccurence(category, occ)
                }

                if (knownSpell === undefined) return undefined

                return new ArchivedSpellOccurence(occ, knownSpell)
            }),
            filter(next => next !== undefined),
            map(next => next as ArchivedSpellOccurence)
        )
    }

    private processNewCreatedSpellOccurence(
        occ: IRawSpellOccurence
    ): ArchivedSpell {
        const spellOcc = occ as IRawCreatedSpellOccurence

        const newSpell: ArchivedSpell = {
            spell: spellOcc.spell,
        }

        this.addActiveSpell(spellOcc.spell.guid, newSpell)

        return newSpell
    }

    private processOtherSpellOccurence(
        category: SpellOccurenceCategory,
        occ: IRawSpellOccurence
    ) {
        const spellOcc = occ as IRawOtherSpellOccurence
        const spellReference = spellOcc.spellReference

        const knownSpell = this.getKnownSpell(spellReference)

        if (!knownSpell) {
            console.error(
                `Got an undefined spell from the archive for guid ${spellReference}`
            )
            return undefined
        }

        if (category == SpellOccurenceCategory.Forgotten) {
            this.setSpellForgotten(spellReference)
        } else if (category == SpellOccurenceCategory.HasMana) {
            // * wauw! this is by reference!
            // * https://stackoverflow.com/questions/6605640/javascript-by-reference-vs-by-value
            // * "However, changing a property of an object referenced by a variable
            // * does change the underlying object."
            
            const knownBindingSpell = knownSpell.spell as ISerlializableBindingSpell
            knownBindingSpell.hasMana = true
        } else if (category == SpellOccurenceCategory.NoMana) {
            const knownBindingSpell = knownSpell.spell as ISerlializableBindingSpell
            knownBindingSpell.hasMana = false
        } else if (category == SpellOccurenceCategory.Wispd) {
            // TODO wispd?
            console.log("dows wispd ever happen? 🙉")
        }

        return knownSpell
    }

    private addActiveSpell(spellGuid: string, newSpell: ArchivedSpell) {
        this.activeSpells.set(spellGuid, newSpell)
    }

    setSpellForgotten(spellGuid: string) {
        const spell = this.getKnownSpell(spellGuid)

        if (spell == undefined) {
            console.error(
                `Attempted removing a spell that doesnt exist or was already removed, guid: ${spellGuid}`
            )
            return
        }

        this.activeSpells.delete(spellGuid)
        this.forgottenSpells.set(spellGuid, spell)
    }

    removeForgottenSpell(spellGuid: string) {
        this.forgottenSpells.delete(spellGuid)
    }

    getSpellCasterName(spellGuid: string): string {
        const knownSpell = this.getKnownSpell(spellGuid)

        if (knownSpell == undefined) {
            this.requested.push(spellGuid)
            return "derp"
        } else {
            return knownSpell.spell.whosName
        }
    }

    getSpellWho(spellGuid: string): Who | undefined {
        return this.getKnownSpell(spellGuid)?.spell.Who
    }

    getDistinctWhoTypes(): string[] {
        const types = new Set<string>()
        for (const s of this.activeSpells.values())
            types.add(s.spell.Who.WhosType.name)
        for (const s of this.forgottenSpells.values())
            types.add(s.spell.Who.WhosType.name)
        return Array.from(types).sort()
    }

    getWhoDescriptionsForType(typeName: string): (string | null)[] {
        const descs = new Set<string | null>()
        const check = (s: ArchivedSpell) => {
            if (s.spell.Who.WhosType.name === typeName)
                descs.add(s.spell.Who.WhosDescription ?? null)
        }
        for (const s of this.activeSpells.values()) check(s)
        for (const s of this.forgottenSpells.values()) check(s)
        return Array.from(descs)
    }

    getSpellTitle(spellGuid: string): string {
        const knownSpell = this.getKnownSpell(spellGuid)

        if (knownSpell == undefined) {
            this.requested.push(spellGuid)
            return "Unknown or Forgotten Spell Title 🪶"
        } else {
            return knownSpell.spell.title
        }
    }

    private getKnownSpell(spellGuid: string): ArchivedSpell | undefined {
        let knownSpell: ArchivedSpell | undefined

        if (this.activeSpells.has(spellGuid)) {
            knownSpell = this.activeSpells.get(spellGuid)
        } else if (this.forgottenSpells.has(spellGuid)) {
            knownSpell = this.forgottenSpells.get(spellGuid)
        }

        return knownSpell
    }
}
