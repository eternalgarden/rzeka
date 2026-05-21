import { Type } from "../../../types/common/Type";

export function getFullMatterType(matterType: Type | undefined): string {
    if (!matterType) return "lol"
    return `${matterType.namespace}.${matterType.name}`
}

export function getFewGuidCharacters(guid: string): string {
  return guid.substring(0, 6);
}

const MAX_DEPTH: number = 3
export function getObjectContents(
    o: any,
    markup: string,
    currentDepth: number
): string {
    let returnedListMarkup: string = ""

    // console.log(o)

    // * filtering out null entries from the deserialized object
    // https://stackoverflow.com/questions/286141/remove-blank-attributes-from-an-object-in-javascript
    o = Object.fromEntries(Object.entries(o).filter(([_, v]) => v != null))

    const keys: string[] = Object.keys(o)

    for (let i = 0; i < keys.length; i++) {
        const key: string = keys[i]
        const el: any = o[key]

        if (typeof el === "object" && currentDepth < MAX_DEPTH) {
            const innerMarkup: string = getObjectContents(
                el,
                returnedListMarkup,
                currentDepth + 1
            )
            returnedListMarkup += `<li>${key}:<ul>${innerMarkup}</ul></li>`
        } else returnedListMarkup += `<li>${key}: ${el}</li>`
    }

    return returnedListMarkup
}