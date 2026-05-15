import { Type } from "../Type"
import { MatterData } from "./MatterData"

export class ArchivedMatter {
    readonly matterType: Type
    readonly matterData: MatterData
    readonly spellReference: string

    receivingSpells: string[] = []

    constructor(matterType: Type, matter: MatterData, spellReference: string) {
        this.matterType = matterType
        this.matterData = matter
        this.spellReference = spellReference
    }

    addReceivingSpell(spellGuid: string) {
        this.receivingSpells.push(spellGuid)
    }
}
