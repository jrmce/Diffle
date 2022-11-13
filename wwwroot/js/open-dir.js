export function openDirectory() {
    return window.showDirectoryPicker();
}
export function compare(dir1, dir2) {
    return dir1.isSameEntry(dir2);
}
export async function getFiles(dir) {
    const fileNames = [];
    for await (const entry of dir.values()) {
        if (entry.kind === 'file') {
            fileNames.push(entry.name);
        }
    }
    return fileNames;
}
export async function getFileContents(dir, fileName) {
    const fileHandle = await dir.getFileHandle(fileName);
    const file = await fileHandle.getFile();
    const text = await file.text();
    const name = file.name;
    return { text, name };
}
//# sourceMappingURL=open-dir.js.map