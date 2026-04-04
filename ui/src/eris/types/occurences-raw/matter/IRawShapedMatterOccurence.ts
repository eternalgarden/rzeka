import { MatterData } from "../../common/matter/MatterData"
import { Type } from "../../common/Type"
import { IRawMatterOccurence } from "./IRawMatterOccurence"

export interface IRawShapedMatterOccurence extends IRawMatterOccurence {
    matterType: Type // * Type of the emitted matter
    matter: MatterData
}
