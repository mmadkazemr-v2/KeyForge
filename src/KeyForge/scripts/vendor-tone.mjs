import { copyFile, mkdir } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const projectDirectory = join(dirname(fileURLToPath(import.meta.url)), '..');
const sourceDirectory = join(projectDirectory, 'node_modules', 'tone', 'build');
const targetDirectory = join(projectDirectory, 'wwwroot', 'lib', 'tone');

await mkdir(targetDirectory, { recursive: true });
await Promise.all([
  copyFile(join(sourceDirectory, 'Tone.js'), join(targetDirectory, 'tone.js')),
  copyFile(join(sourceDirectory, 'Tone.js.LICENSE.txt'), join(targetDirectory, 'LICENSE.txt')),
]);

console.log('Tone.js 15.1.22 assets copied to wwwroot/lib/tone.');
