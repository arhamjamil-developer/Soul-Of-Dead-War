from PIL import Image
from pathlib import Path
from collections import deque
r=Path('B:/Soul-Of-Dead-War/Assets/UI/Final/Production/Frames')
for p in r.glob('*.png'):
 im=Image.open(p).convert('RGBA');w,h=im.size;pix=im.getchannel('A').tobytes();seen=bytearray(w*h);best=(0,None)
 for start,v in enumerate(pix):
  if v<180 or seen[start]:continue
  seen[start]=1;q=deque([start]);n=0;left=w;right=top=0;bottom=0;top=h
  while q:
   a=q.popleft();x=a%w;y=a//w;n+=1;left=min(left,x);right=max(right,x);top=min(top,y);bottom=max(bottom,y)
   for b in (a-1 if x>0 else -1,a+1 if x<w-1 else -1,a-w if y>0 else -1,a+w if y<h-1 else -1):
    if b>=0 and not seen[b] and pix[b]>=180:seen[b]=1;q.append(b)
  if n>best[0]:best=(n,(max(0,left-2),max(0,top-2),min(w,right+3),min(h,bottom+3)))
 if best[1]:im=im.crop(best[1]);im.save(p)
 print(p.name,(w,h),'->',im.size)
