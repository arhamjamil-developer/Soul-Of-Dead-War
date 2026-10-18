const { chromium }=require('C:/Users/ASUS/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const fs=require('fs');const path=require('path');
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true,args:['--allow-file-access-from-files','--autoplay-policy=no-user-gesture-required']});
 const page=await browser.newPage({viewport:{width:720,height:1200},deviceScaleFactor:1});let errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.goto('file:///'+path.join(__dirname,'Soul-Crystal-Animated-Mockup.html').replaceAll('\\','/'));
 await page.waitForFunction(()=>window.mockupReady);
 for(const [name,time]of [['Battle',3],['Corruption',5.3],['SoulPulse',7.65],['CrystalStorm',13.3],['Boss',17.5]]){
  await page.evaluate(time=>window.setMockupTime(time),time);
  const data=await page.locator('#game').evaluate(c=>c.toDataURL().split(',')[1]);
  fs.writeFileSync(path.join(__dirname,`Gameplay-${name}-1080x1920.png`),Buffer.from(data,'base64'));
 }
 await page.evaluate(()=>window.setMockupTime(3));
 let rect=await page.locator('#game').boundingBox();
 await page.mouse.click(rect.x+120/1080*rect.width,rect.y+1750/1920*rect.height);
 let selected=await page.evaluate(()=>window.mockupState().selected);
 await page.mouse.click(rect.x+180/1080*rect.width,rect.y+800/1920*rect.height);
 let placed=await page.evaluate(()=>window.mockupState().placements);
 await page.mouse.click(rect.x+300/1080*rect.width,rect.y+1615/1920*rect.height);
 let casts=await page.evaluate(()=>window.mockupState().casts);
 const sizes=[];
 for(const width of [320,390,768]){
  await page.setViewportSize({width,height:1100});
  sizes.push(await page.evaluate(()=>({width:innerWidth,overflow:document.documentElement.scrollWidth>innerWidth,canvas:document.querySelector('canvas').getBoundingClientRect().width})));
  await page.screenshot({path:path.join(__dirname,`QA-Viewport-${width}.png`),fullPage:true});
 }
 await page.setViewportSize({width:720,height:1200});
 await page.locator('#restart').click();
 fs.writeFileSync(path.join(__dirname,'QA.json'),JSON.stringify({errors,selected,placed,casts,sizes},null,2));
 if(errors.length||selected!==0||placed!==1||casts!==1||sizes.some(s=>s.overflow))throw Error('Mockup QA failed');
 console.log('QA passed: card selection, deployment, crystal ability and 320/390/768 layouts. Recording...');
 const video=await page.evaluate(()=>window.recordMockup(22));
 fs.writeFileSync(path.join(__dirname,'Gameplay-Animated-Mockup.webm'),Buffer.from(video,'base64'));
 await browser.close();console.log('Animation recorded.');
})().catch(e=>{console.error(e);process.exit(1)});
