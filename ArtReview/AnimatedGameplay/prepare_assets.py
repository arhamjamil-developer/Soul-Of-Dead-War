from pathlib import Path
from PIL import Image
import json,base64,io

ROOT=Path(__file__).resolve().parents[2]
OUT=Path(__file__).resolve().parent
S=ROOT/'Assets/SHAMSA'
paths={
 'dwarf':S/'Assets/Tortured_manSprites/Barbarian_Attack.png',
 'chef':S/'Assets/Chef/Chef_Attack.png',
 'raider':S/'Assets/Enemies/BoneRaider/BoneRaider_Down_Animations.png',
 'reaper':S/'Assets/Enemies/SoulReaper/SoulReaper_Down_Animations.png',
 'crystal':S/'Visuals/SoulCrystal_Sprite.png',
 'elephant':S/'Assets/Elephant/Armored Royal Elephant Rotation Sheet.png',
 'dwarfPortrait':S/'Assets/Tortured_manSprites/Top-Down Dwarf Warrior Rotation Sheet.png',
 'chefPortrait':S/'Assets/Chef/Bloodstained Chef Rotation Sprite Sheet.png',
}
assets={}
def encode(im):
 b=io.BytesIO();im.save(b,format='PNG',optimize=True)
 return 'data:image/png;base64,'+base64.b64encode(b.getvalue()).decode()

def frames(name,path,n,rows=1):
 im=Image.open(path).convert('RGBA');w,h=im.size;fw,fh=w//n,h//rows
 # Retain the cell coordinate system and all original animation frame timing.
 frames=[]
 for i in range(n):
  tile=im.crop((i*fw,0,(i+1)*fw,fh))
  tile.thumbnail((160,256),Image.Resampling.LANCZOS)
  frames.append(encode(tile))
 assets[name]=frames

for name in ('dwarf','chef'):frames(name,paths[name],6)
for name in ('raider','reaper'):frames(name,paths[name],8,3)
for name in ('dwarfPortrait','chefPortrait','elephant'):
 im=Image.open(paths[name]).convert('RGBA');w,h=im.size
 # Front-facing portrait in rotation sheet; top-down boss uses front-facing pose.
 idx=3 if name=='elephant' else 3
 im=im.crop((idx*w//6,0,(idx+1)*w//6,h));box=im.getbbox()
 if box:im=im.crop(box)
 im.thumbnail((250,330),Image.Resampling.LANCZOS);assets[name]=encode(im)
im=Image.open(paths['crystal']).convert('RGBA');assets['crystal']=encode(im)
im=Image.open(OUT/'assets/battlefield.png').convert('RGB');im.thumbnail((1080,1440),Image.Resampling.LANCZOS)
assets['board']=encode(im)
new=OUT/'assets/new-guardians.png'
if new.exists():
 im=Image.open(new).convert('RGBA');w,h=im.size
 for i,name in enumerate(('archer','mage')):
  tile=im.crop((i*w//2,0,(i+1)*w//2,h));box=tile.getbbox()
  if box:tile=tile.crop(box)
  tile.thumbnail((220,330),Image.Resampling.LANCZOS);assets[name]=encode(tile)
template=(OUT/'mockup.template.html').read_text(encoding='utf-8')
(OUT/'Soul-Crystal-Animated-Mockup.html').write_text(template.replace('__ASSET_DATA__',json.dumps(assets)),encoding='utf-8')
print('Self-contained animated mockup built:',OUT/'Soul-Crystal-Animated-Mockup.html')
