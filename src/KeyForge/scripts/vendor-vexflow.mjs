import { copyFile, mkdir } from 'node:fs/promises';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const projectDirectory = join(dirname(fileURLToPath(import.meta.url)), '..');
const sourceDirectory = join(projectDirectory, 'node_modules', 'vexflow');
const targetDirectory = join(projectDirectory, 'wwwroot', 'lib', 'vexflow');

await mkdir(targetDirectory, { recursive: true });
await Promise.all([
  copyFile(
    join(sourceDirectory, 'build', 'cjs', 'vexflow-bravura.js'),
    join(targetDirectory, 'vexflow-bravura.js')),
  copyFile(
    join(sourceDirectory, 'LICENSE'),
    join(targetDirectory, 'LICENSE.txt')),
]);

console.log('VexFlow 5.0.0 assets copied to wwwroot/lib/vexflow.');
