// TODO This is not super justified to be bundled into an actual component
// TODO Find a way to just fetch host json files with webpack
// TODO And preferably avoid that in production
import testData from "./../../assets/test-data/realmEventsData.json?raw"

export function loadTestDataIfInDebugMode() {
    // * since I am debugging in firefox and unity plugin uses chromium

    try {
        const href = window.location.href

        if (href.includes("#debug")) {
            console.log("Mozilla Firefox")

            let debugs = Array.from(document.getElementsByClassName("debug"))

            debugs.forEach(el => {
                ;(el as HTMLElement).style.visibility = "visible"
            })
        }

        if (href.includes("#local")) {
            // ? if the size becomes problematic one day
            // ? consider this: https://github.com/dominictarr/JSONStream
            let data: [] = JSON.parse(testData).data
            // console.log(data)
            data.forEach(o => window.OnNewOccurence(o))
        }
    } catch (error) {
        throw error
    }
}