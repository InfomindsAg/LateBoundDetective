# LateBoundDetective
Searches for code, that results in unnecessary late bound calls.

* CreateInstance with known Types -> replace it with a call of the constuctor 
* SafeCreateInstance with known Types -> replace it with a call of the constuctor 
* GetRegServerReal():Open with known Types -> replace it with GetRegServerReal():OpenTyped<>
