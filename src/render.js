const { chromium } = require('playwright');
const path = require('path');
(async () => {
  const dir = process.argv[2];
  const b = await chromium.launch();
  const p = await b.newPage();
  for (const n of ['TP2', 'TP3']) {
    await p.goto('file://' + path.join(dir, n + '.html'));
    await p.evaluate(() => document.fonts.ready);
    const over = await p.evaluate(() => [...document.querySelectorAll('.body')].map((e, i) =>
      e.scrollHeight > e.clientHeight ? `page ${i + 2}: ${e.scrollHeight}/${e.clientHeight}` : null).filter(Boolean));
    console.log(n, over.length ? 'OVERFLOW ' + over.join(', ') : 'ok');
    await p.pdf({ path: path.join(dir, n + '.pdf'), preferCSSPageSize: true, printBackground: true });
  }
  await b.close();
})();
