import { ISerializableSpell } from "./ISerializableSpell"

export class ArchivedSpell {
    spell: ISerializableSpell

    constructor(spell: ISerializableSpell, hasMana: boolean) {
        this.spell = spell
    }
}
