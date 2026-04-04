import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IRawSpellOccurence } from "../occurences-raw/spell/IRawSpellOccurence"
import { IArchivedOccurence } from "./IArchivedOccurence"
import { ArchivedSpell } from "../common/spells/ArchivedSpell"

export class ArchivedSpellOccurence implements IArchivedOccurence {
    readonly occurenceCategory: OccurenceCategory = OccurenceCategory.Spell
    readonly guid: string
    readonly timestamp: number

    private readonly _occurence: IRawSpellOccurence
    private readonly _knownSpell: ArchivedSpell

    public get occurence(): IRawSpellOccurence {
        return this._occurence
    }
    public get knownSpell(): ArchivedSpell {
        return this._knownSpell
    }

    constructor(occurence: IRawSpellOccurence, knownSpell: ArchivedSpell) {
        this.guid = occurence.guid

        this._occurence = occurence
        this._knownSpell = knownSpell
    }
}
