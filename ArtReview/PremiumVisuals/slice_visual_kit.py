from PIL import Image
from pathlib import Path
import json, shutil
ROOT=Path(__file__).resolve().parents[2]
SRC=Path('C:/Users/ASUS/.codex/generated_images/01a14fef-7286-7931-aa1f-ce1d66ca2e10')
OUT=ROOT/'Assets/UI/Premium'
OUT.mkdir(parents=True,exist_ok=True)
raw=ROOT/'Assets/UI/Extra Ui Images/Sprite Sources';raw.mkdir(parents=True,exist_ok=True)
jobs=[
 ('Frames','exec-7f32ff57-9796-4ca1-8572-b091c92b684d.png',2,3,['Panel','TitleBanner','Button','CardFrame','SliderTrack','SelectionFrame']),
 ('Icons','exec-eb5bdc95-ad08-4235-b8fb-cdb531617398.png',4,4,['Crystal','Pulse','Storm','Barrier','Pause','Play','Settings','Back','Volume','Music','Owned','Exit','Elephant','Train','Book','Trophy']),
 ('Portraits','exec-b6d60263-ef25-4c23-b7c0-064ad68dcaa8.png',2,3,['Dwarf','Chef','Archer','Mage','Paladin','Witch']),
 ('Supports','exec-101ed860-927f-4114-a906-3a7d58f4eec8.png',2,2,['ElephantPortrait','TrainPortrait','ElephantWorld','TrainWorld'])
]
manifest=[]
for group,filename,cols,rows,names in jobs:
 im=Image.open(SRC/filename).convert('RGBA');w,h=im.size
 shutil.copy2(SRC/filename,raw/(group+'-Source.png'))
 folder=OUT/group;folder.mkdir(exist_ok=True)
 for n,name in enumerate(names):
  col,row=n%cols,n//cols
  tile=im.crop((round(col*w/cols),round(row*h/rows),round((col+1)*w/cols),round((row+1)*h/rows)))
  box=tile.getchannel('A').point(lambda v:255 if v>=24 else 0).getbbox()
  if box:tile=tile.crop((max(0,box[0]-3),max(0,box[1]-3),min(tile.width,box[2]+3),min(tile.height,box[3]+3)))
  dest=folder/(name+'.png');tile.save(dest)
  manifest.append({'name':name,'group':group,'path':str(dest.relative_to(ROOT)).replace('\\','/'),'size':tile.size,'source':filename,'cell':n})
(OUT/'SpriteManifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Saved',len(manifest),'individual UI and character sprites.')
