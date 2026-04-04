import {
    css,
    customElement,
    FASTElement,
    html,
    observable,
    ref,
} from "@microsoft/fast-element"
import { Subscription, fromEvent, map } from "rxjs"
import brotliPromise from "brotli-wasm"
import { Buffer } from "buffer/"

const styles = css`
    ::part(container) {
        border-width: 1px;
        border-style: dashed none none none;
    }

    ::part(content) {
        padding: 1rem;
        font-family: monospace;
        color: white;
    }

    input {
        font-family: monospace;
    }
`

const template = html<WHDIGLoader>`
    <sun-container
        draggable
        emoji="🪴"
        color="rgb(41, 50, 56)"
        top="35%"
        left="50%">
        <p>Select a WHDIG.txt file to load its data.</p>
        <input
            type="file"
            name="inputfile"
            ${ref("inputfile")} />
    </sun-container>
`

export class WHDIGLoader extends FASTElement {
    @observable inputfile: HTMLInputElement

    inputSubscription: Subscription

    async connectedCallback(): Promise<void> {
        super.connectedCallback() // ! don't forget this
    }

    inputfileChanged(old: HTMLInputElement, newInput: HTMLInputElement) {
        if (this.inputSubscription !== undefined)
            this.inputSubscription.unsubscribe()

        this.inputSubscription = fromEvent(newInput, "change")
            .pipe(
                map(x => {
                    let fr = new FileReader()

                    if (newInput.files != null && newInput.files.length > 0) {
                        fr.readAsText(newInput.files[0])
                    }

                    return fr
                })
            )
            .subscribe(fileReader => {
                fileReader.onload = async function () {
                    const result = fileReader.result as string
                    const regex = new RegExp(
                        "@rzeka_story\r\n(?<story>.*)\r\n&rzeka_story",
                        "s"
                    )
                    const exec = regex.exec(result)

                    if (exec?.groups != null) {
                        const story = exec.groups["story"]

                        const brotli = await brotliPromise // Import is async in browsers due to wasm requirements!
                        const textDecoder = new TextDecoder()
                        const decompressedData: Uint8Array = brotli.decompress(
                            Buffer.from(story, "base64")
                        )

                        const arr = JSON.parse(
                            textDecoder.decode(decompressedData)
                        )
                        // TODO add clear occurences
                        // 
                        window.ClearOccurences();
                        
                        for (let i = 0; i < arr.length; i++) {
                            const element = arr[i]

                            window.OnNewOccurence(element)
                        }

                        console.log(arr)
                    }
                }
            })
    }
}

WHDIGLoader.define({
    name: "whdig-loader",
    template,
    styles
})