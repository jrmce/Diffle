export function openDirectory() {
    return (window as any).showDirectoryPicker();
}

export function compare(dir1: FileSystemDirectoryHandle, dir2: FileSystemDirectoryHandle) {
    return dir1.isSameEntry(dir2);
}

export async function getFiles(dir: any) {
    const fileNames: string[] = [];
    for await (const entry of dir.values()) {
        if (entry.kind === 'file') {
            fileNames.push(entry.name);
        }
    }
    return fileNames;
}

export async function getFileContents(dir: FileSystemDirectoryHandle, fileName: string) {
    const fileHandle = await dir.getFileHandle(fileName);
    const file = await fileHandle.getFile();
    const text = await file.text();
    const name = file.name;
    return { text, name };
}