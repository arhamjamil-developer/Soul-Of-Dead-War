from pathlib import Path
from PIL import Image
import json,shutil
root=Path('B:/Soul-Of-Dead-War');src=Path('C:/Users/ASUS/.codex/generated_images/01a14fef-7286-7931-aa1f-ce1d66ca2e10');out=root/'Assets/UI/Final/Production';out.mkdir(exist_ok=True)
manifest=[]
def save_crop(image,box,group,name,tight=True):
 im=image.crop(box).convert('RGBA')
 if tight:
  bb=im.getchannel('A').point(lambda a:255 if a>40 else 0).getbbox()
  if bb:im=im.crop((max(0,bb[0]-2),max(0,bb[1]-2),min(im.width,bb[2]+2),min(im.height,bb[3]+2)))
 p=out/group/(name+'.png');p.parent.mkdir(exist_ok=True);im.save(p);manifest.append({'name':name,'path':str(p.relative_to(root)),'size':im.size})
im=Image.open(src/'exec-c150bff0-fb4e-40cc-a88c-00fcd7c3c8a9.png');print('kit',im.size)
sx=im.width/1254;sy=im.height/1254
boxes={'GoldPanel':(32,140,685,445),'CardHolder':(780,75,1100,515),'CostPill':(30,620,635,845),'Tray':(640,585,1230,880),'Inset':(30,975,425,1155),'HintBanner':(430,950,1240,1190)}
for name,b in boxes.items():save_crop(im,tuple(round(v*(sx if i%2==0 else sy)) for i,v in enumerate(b)),'Frames',name)
im=Image.open(src/'exec-47ded451-bc0d-45a1-95e1-6b1fc2c65beb.png');print('icons',im.size)
for i,name in enumerate(['Crystal','Pulse','Storm','Barrier','Pause','Gem','Elephant','Train']):
 col=i%4;row=i//4;x0=round(col*im.width/4);x1=round((col+1)*im.width/4);y0=round(row*im.height/2);y1=round((row+1)*im.height/2)
 if name=='Elephant':x0=round(im.width*.46);y0=round(im.height*.52)
 if name=='Train':x0=round(im.width*.745);y0=round(im.height*.54)
 save_crop(im,(x0,y0,x1,y1),'Icons',name)
im=Image.open(src/'exec-91f6a29d-789b-44b0-94f5-50bd418d4b21.png');print('portraits',im.size)
for i,name in enumerate(['Dwarf','Chef','Archer','Mage','Paladin','Witch']):
 col=i%3;row=i//3;save_crop(im,(round(col*im.width/3),round((0 if row==0 else .52)*im.height),round((col+1)*im.width/3),round((.518 if row==0 else 1)*im.height)),'Portraits',name)
world=out/'World';world.mkdir(exist_ok=True);shutil.copy2(src/'exec-3d722f74-8bbd-4e89-8c10-5fc13e99f15c.png',world/'Graveyard-Concept.png')
im=Image.open(world/'Graveyard-Concept.png');w,h=im.size
# Texture-only crops for the forty individually authored board patches.
for i,(x,y) in enumerate([(.43,.34),(.29,.34),(.58,.34),(.72,.34)]):save_crop(im,(int(x*w),int(y*h),int((x+.135)*w),int((y+.058)*h)),'World','StonePatch_'+str(i),False)
(out/'SpriteManifest.json').write_text(json.dumps(manifest,indent=2))
print('Saved',len(manifest),'individual sprites')
