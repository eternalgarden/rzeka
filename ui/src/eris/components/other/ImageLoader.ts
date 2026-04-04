import {
    css,
    customElement,
    FASTElement,
    html,
    observable,
    ref,
} from "@microsoft/fast-element"
import { Subscription, fromEvent, map } from "rxjs"

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
        background-color: red;
    }

    button:active {
        background-color: blue;
    }
`

const template = html<ImageLoader>`
    <sun-container
        draggable
        emoji="⛺"
        color="rgb(41, 50, 56)"
        top="70%"
        left="40%">
        <p>Select a test image file.</p>
        <input
            type="file"
            name="inputfile"
            accept="image/png, image/jpeg"
            ${ref("inputfile")} />
            <button>LOAD IMAGE</button
    </sun-container>
`

export class ImageLoader extends FASTElement {
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
                        // fr.readAsText(newInput.files[0])
                        fr.readAsDataURL(newInput.files[0])
                    }

                    return fr
                })
            )
            .subscribe(fileReader => {
                fileReader.onload = async function () {
                    console.log(fileReader.result)

                    const result = fileReader.result as string
                    console.log(result)

                    // https://stackoverflow.com/questions/38633061/how-can-i-strip-the-dataimage-part-from-a-base64-string-of-any-image-type-in-ja
                    const data = result.split("base64,")[1]
                    console.log(data)

                    // const brotli = await brotliPromise
                    // const textecoder = new TextEncoder()
                    // const encoded = textecoder.encode(data);
                    // const compressed = brotli.compress(encoded)

                    // var base64String = window.btoa(String.fromCharCode.apply(null, compressed));

                    window.OnImageData(data)
                }
            })
    }
}

ImageLoader.define({
    name: "image-loader",
    template,
    styles,
    shadowOptions: { mode: "open", delegatesFocus: true },
})