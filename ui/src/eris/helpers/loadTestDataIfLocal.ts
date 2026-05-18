// TODO This is not super justified to be bundled into an actual component
// TODO Find a way to just fetch host json files with webpack
// TODO And preferably avoid that in production
import testData from "./../../assets/test-data/realmEventsData.json?raw"

export function loadTestDataIfLocal() {
    try {
        const href = window.location.href

        if (href.includes("#local")) {
            // ? if the size becomes problematic one day
            // ? consider this: https://github.com/dominictarr/JSONStream
            let data: [] = JSON.parse(testData).data
            data.forEach(o => window.OnNewOccurence(o))
        }
    } catch (error) {
        throw error
    }
}
