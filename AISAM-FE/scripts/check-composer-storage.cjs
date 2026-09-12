const { chromium } = require('playwright');
const ts = require('typescript');
const fs = require('fs');
(async () => {
  const browser = await chromium.launch({ headless: true, channel: process.env.COMPOSER_BROWSER_CHANNEL || "msedge" });
  try {
    const page = await browser.newPage();
    await page.route('http://composer.test/**', route => route.fulfill({ contentType: 'text/html', body: '<html></html>' }));
    await page.goto('http://composer.test/');
    const code = ts.transpileModule(fs.readFileSync('src/lib/composerFiles.ts', 'utf8'), { compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2020 } }).outputText;
    await page.evaluate(code => { const exports = {}; new Function('exports', code)(exports); window.storageTest = exports; }, code);
    await page.evaluate(async () => {
      await window.storageTest.storeComposerFiles('actor:workspace:content', [{ id: 'file1', file: new File(['abc'], 'clip.mp4', { type: 'video/mp4' }), status: 'Uploading' }]);
    });
    await page.reload();
    await page.evaluate(code => { const exports = {}; new Function('exports', code)(exports); window.storageTest = exports; }, code);
    await page.evaluate(async () => {
      const rows = await window.storageTest.loadComposerFiles('actor:workspace:content');
      if (rows.length !== 1 || rows[0].status !== 'Pending' || rows[0].file.name !== 'clip.mp4' || await rows[0].file.text() !== 'abc') throw Error('File round trip failed');
      if ((await window.storageTest.loadComposerFiles('other:workspace:content')).length) throw Error('Actor isolation failed');
      await window.storageTest.storeComposerFiles('actor:workspace:content', [{ ...rows[0], status: 'Done' }]);
      if ((await window.storageTest.loadComposerFiles('actor:workspace:content')).length) throw Error('Completed file not removed');
    });
    process.stdout.write('PASS Chromium IndexedDB file reload, actor isolation and completed-file cleanup\n');
  } finally { await browser.close(); }
})().catch(e => { console.error(e.message); process.exitCode = 1; });