; Remember to DISABLE Debug :)
;
; Gravity Force II and Gravity Power were originally written by:
; 	Jens Andersson & Jan Kronqvist
;
; BlitzPC version by Richard Franks, spontificus@yahoo.com
;
; Beta V0.6 -> For latest updates check out:
;	http://fly.to/GP2
;
; If you spot any bugs, chances are that there's a version with them fixed at the web-site
;
;
; F1 to change levels
; F4 to toggle debugging
; F5-F6 for different views
; F9 Save screen
; Esc to quit
;
; Player 1
; LEFT	- Rotate Left | select rear weapon when landed
; RIGHT	- Rotate Right | select forward weapon when landed
; UP	- Fire Cannon
; DOWN	- Second Weapon
; RCTRL - Thrust!
; kpad0	- Air Brake | reverse weapon selection (temporary command)

; Player 2
; Z		- Rotate Left | select rear weapon when landed
; C		- Rotate Right | select forward weapon when landed
; S		- Fire Cannon
; X		- Second Weapon
; L-ALT	- Thrust!
; V		- Air Brake | reverse weapon selection (temporary command)

AppTitle ("GP2 v0.6")

Gosub DoIncludes

; Init some variables for playing around with

Global DEBUG=True

;prefgfx_x = 800		; change this to 0 for no preference
prefgfx_y = 600

;prefgfx_x = 640
prefgfx_y = 480



autorefuel = 1		; 0 = refuel only on pads, 1 = refuel constantly (for testing or wimpy players)
softplay = 1		; only used when autorefuel=1, 0 = default autorefuel,
					;							   1 = recharge only shields, but slowly
Global numrots = 32	; original = 32
Const amodf# = 0.3	; if using 32 rotations then 0.3 here is quite good

gravity# = 0.01		; <-\
thrust#=0.028		; play with these to get 'original' feel

maxbullets = 50		; max bullets
firedelay = 3		; lower = less delay
firerate# = 2		; higher = more per second

Const maxshipinc# = 1.0 ; how many pixels horizontally and vertically can the ship move per frame?
Const maxshipfallinc# = 1.5

maxnumtypes=20		; how many different level types can be supported
maxlevels=500		; how many levels of a certain type can be supported?

maxfuel=15000:	deffuel=5000:	strtfuel=deffuel:		fuelinc#=1000/78.0
maxshield=100:	defshield=30:	strtshield=defshield:	shieldinc#=20/78.0
maxammo=1000:	defammo=300:	strtammo=defammo:		ammoinc#=200/78.0


; define keys
Dim keyleft(2)	:keyleft(0)=203	:keyleft(1)=44
Dim keyright(2)	:keyright(0)=205:keyright(1)=46
Dim keyup(2)	:keyup(0)=200	:keyup(1)=31
Dim keydwn(2)	:keydwn(0)=208	:keydwn(1)=45
Dim keythrst(2)	:keythrst(0)=157:keythrst(1)=56
Dim keystop(2)	:keystop(0)=82	:keystop(1)=47




; some variables not really for playing around with :)

; with all the recent hacks, making this a one player game would take a while
; but there are plans to clean up and make it 1-4 player at least
Global numplayers = 2 		; how many players? (1-2)

maxweap = 100

Global maxsounds=10		; increase for more sounds!
Global maxchannels=2	; how many instances of *each* sound are supported?

Dim sounds.soundtype(maxsounds)
Dim ammorot(maxweap,numrots+1)
Dim weapon.WeaponType(maxweap)


Gosub InitDisplay		; SETUP Display
menu=1

dir$ = "gfx/GR/"
SeedRnd(MilliSecs())

max_shapes = 300
Global numshapes = 0
shipang# = 0
Global dispheight = (GraphicsHeight() - 32)/numplayers
Global map
Global supermap
Global water_s#
Global water_e
Global water_spd#
Global gpfont

Global numsounds=0



;Dim dispoff(2)
;dispoff(0) = 0
;dispoff(1) = GraphicsHeight() - dispheight

Global shipangmod# = 360/numrots


Type rot
	Field x#,y#
End Type
Dim rots.rot(200)

Type airobj
	Field x#
	Field y#
	Field ox#
	Field oy#
	Field xinc#,yinc#
	Field weight
	Field dead
	Field player
	Field antigrav
	Field s.shooter
	Field pang#
	Field oang#
	Field wi.WeaponInstance
End Type

Type shooter
	Field pretimer
	Field timer
	Field timeout
	Field x,y
	Field mx,my
	Field dx,dy
	Field ex,ey
	Field dir
	Field checked
	Field inv
	Field dead
End Type

Type magnet
	Field x,y,w,h, dir
End Type

Type ship
	Field x#
	Field y#
	Field ox#
	Field oy#
	Field xinc#,yinc#
	Field landed
	Field onpad
	Field player
	Field ang#, oang#
	Field shipstate, oshipstate
	Field weap1,weap2	
End Type

Type playertype
	Field xpos#,ypos#
	Field score
	Field shield#
	Field fuel#
	Field ammo1#
	Field reloadammo1
	Field numweap1
	Field ammo2#
	Field reloadammo2
	Field numweap2
	Field lives
	Field numbullets
	Field seep
	Field fireround
	Field firedelay
	Field firerate
	Field dead
	Field underwater
	Field keylock
	Field lap,pos
	Field v.vport
	Field vs.vport
	Field state
;	Field score
End Type

Type podtype
	Field x,y,w,h
End Type


Type levtypes
	Field ap$, bitmap$, name$, title$, author$
	Field numshapes,numlevels
	Field shapeslot
	Field loaded
	Field lp1,lp2,lp3
End Type

lv.levtypes=Null

;Dim levtype.levtypes(20)

Type rec
	Field x,y,width,height
End Type

Dim recs.rec(2)
recs(0) = New rec
recs(1) = New rec

Dim player.playertype(2)
player(0) = New playertype
player(1) = New playertype
player(2) = New playertype ; for shooters

fpsv.vport = New vport

player(0)\vs = New vport
player(1)\vs = New vport

player(0)\v = New vport
player(1)\v = New vport


fpsv\x = GraphicsWidth()/2
fpsv\y = GraphicsHeight()/2
fpsv\w = 200
fpsv\h = 30
fpsv\hide = 1

Dim ships.ship(4)
ships(0) = New ship
ships(1) = New ship
;ships(curship)\x = 20

; dim some arrays
;Dim levelfil(30000)
;Dim level(30000)


Dim shapes(max_shapes,2,maxnumtypes)
Dim minshapes(max_shapes)
Dim shipshape(128+1,9)
Dim dpckout(40000,3)
Dim dpckin(40000,3)
Dim quotes$(200)
Dim cols(16,3)
Dim mcols(16,3,maxnumtypes)
Dim levelnames$(maxlevels,maxnumtypes)
Dim pod.podtype(4)
Global numpods=0
Dim lap(4,2)
Dim charfont(95,8)
Dim charwidth(95)
Dim charcol(8,3)

Dim playerfire(numplayers,maxweap)
Dim kaboomanim(20)

Dim levs.levtypes(maxnumtypes)

xoff = GraphicsWidth()/2 - 336/2

player(0)\v\x = xoff
player(0)\v\y = 0
player(0)\v\w = 336
player(0)\v\h = dispheight

player(0)\vs\x = xoff+336
player(0)\vs\y = 0
player(0)\vs\w = 64
player(0)\vs\h = 100

player(1)\v\x = xoff
player(1)\v\y = GraphicsHeight() - dispheight
player(1)\v\w = 336
player(1)\v\h = dispheight

player(1)\vs\x = xoff+336
player(1)\vs\y = GraphicsHeight() - dispheight
player(1)\vs\w = 64
player(1)\vs\h = 100

pod(0)=New podtype:pod(1)=New podtype:pod(2)=New podtype:pod(3)=New podtype



.main
	Gosub InitDebug
	Gosub doquotes
	Gosub ReadTypes
	Gosub ReadWeapons
	Gosub InitPlayers
	
	DoFont()
	Gosub LoadSounds
	Gosub LoadGFX

	If menu = 1 Then
		Gosub TypeMenu
		Gosub LevelMenu
	Else
		Gosub LoadLevel
	EndIf

	For i=0 To 1
		ships(i)\weap1 = GetWeapon("Standard Cannon")
		ships(i)\weap2 = GetWeapon("Standard Bomb")
	Next

	Gosub Control

	FreeImage(map):map=0
	End


.InitDebug
	CreateDebug( "Thrust0", 15, 0, 0 )
;	CreateDebug( "Thrust1", 15, 300, 0 )
;	CreateDebug( "Landing", 3, 200, 0 )
	CreateDebug( "Colour", 17, 200, 0 )
	CreateDebug( "RCol", 17, 100, 0 )
Return


.InitPlayers
	For i=0 To 1
		Gosub InitPlayer		
	Next
Return


.InitPlayer
		player(i)\numweap1=0
		player(i)\numweap2=0
		player(i)\shield=strtshield
		player(i)\fuel=strtfuel
		player(i)\ammo1=weapon(ships(i)\weap1)\load
		player(i)\ammo2=weapon(ships(i)\weap2)\load
		player(i)\lap = 0
		player(i)\pos = 0
		
		ships(i)\onpad = 1
		ships(i)\xinc = 0
		ships(i)\yinc = 0
		ships(i)\ang = 0
;		ships(i)\weap1 = GetWeapon("Standard Cannon")
;		ships(i)\weap2 = GetWeapon("Standard Bomb")
		
		lap(0,i)=0
Return


.InitDisplay
	If prefgfx_x <> 0
		If GetGraphics(prefgfx_x,prefgfx_y,16) = False
			prefgfx_x = 0
		EndIf
	EndIf	
	
	If prefgfx_x = 0
		If GetGraphics(400, 300, 16) = False
			If GetGraphics(640,480,16) = False
				If GetGraphics(800,600,16) = False
					Print "Can't find any graphics mode to run in!"
					End
				EndIf
			EndIf
		EndIf
	EndIf
	
	panel = LoadImage("gfx/panel.bmp")
	
	SetDisplay(BackBuffer(), Null)

	supermap = CreateImage(336,1008)
Return


Function GetGraphics(width, height, depth)
	For d = depth To 32 Step 8
		If FindGfxMode(width, height, d)
			Graphics width, height, d
			Return True
		EndIf
	Next
	Return False
End Function


Function GetWeapon( name$ )
	For w.weapontype = Each weapontype
		If w\name = name
			Return w\id
		EndIf
	Next
	Return 0
End Function


.Control

	lap(0,0) = 0
	lap(0,1) = 0
	
	moosex = MouseX()
	moosey = MouseY()
	
	vp.vport = Null

	tchk = MilliSecs()

	While KeyDown(1)=0
		round = round + 1
		
		SetBuffer ImageBuffer(map)

		For curship = 0 To numplayers-1
			If player(curship)\dead > 0
				player(curship)\dead = player(curship)\dead - 1
				Goto skip
			EndIf
		
			DoDebug ( "Thrust"+curship, 1, ships(curship)\yinc )
		
			xpos# = player(curship)\xpos
			ypos# = player(curship)\ypos
			shipang# = ships(curship)\ang


			; Fire!
			If KeyDown(keyup(curship))
				LaunchWeapon(curship, ships(curship)\weap1, round,0)
			End If

			If KeyDown(keydwn(curship))
				LaunchWeapon(curship, ships(curship)\weap2, round,1)
			EndIf
			
		
			If KeyDown(keyleft(curship)) And ships(curship)\landed = 0	;left
				shipang = (shipang + numrots-amodf) Mod numrots
			EndIf
		
			If KeyDown(keyright(curship)) And ships(curship)\landed = 0	;right
				shipang = (shipang + amodf) Mod numrots
			EndIf


		
			If ships(curship)\onpad=1
				If KeyDown(keyright(curship))
					If player(curship)\keylock=0
						wp = ships(curship)\weap1
						If KeyDown(keystop(curship))
							wp = wp - 1:If wp < 0 Then wp=wp+numweap
							While weapon(wp)\fitting = "back"
								wp = wp - 1:If wp < 0 Then wp=wp+numweap
							Wend
						Else
							wp = (wp + 1) Mod numweap
							While weapon(wp)\fitting = "back"
								wp = (wp + 1) Mod numweap
							Wend
						EndIf
						ships(curship)\weap1 = wp
						player(curship)\numweap1 = 0
						player(curship)\ammo1 = 0 ; weapon(ships(curship)\weap1)\load
						player(curship)\keylock = 1
					EndIf
				ElseIf KeyDown(keyleft(curship))
					If player(curship)\keylock=0
						wp = ships(curship)\weap2
						If KeyDown(keystop(curship))
							wp = wp - 1:If wp < 0 Then wp=wp+numweap
							While weapon(wp)\fitting = "front"
								wp = wp - 1:If wp < 0 Then wp=wp+numweap
							Wend							
						Else
							wp = (wp + 1) Mod numweap
							While weapon(wp)\fitting = "front"
								wp = (wp + 1) Mod numweap
							Wend
						EndIf
						ships(curship)\weap2 = wp
						player(curship)\numweap2 = 0
						player(curship)\ammo2 = 0 ; weapon(ships(curship)\weap2)\load
						player(curship)\keylock = 1
					EndIf
				Else
					player(curship)\keylock = 0
				EndIf
			EndIf
			
	
			; keep window nicely positioned
			If ships(curship)\x-xpos<0.4*player(curship)\v\w Then
				If xpos > 0 Then
					If ships(curship)\x-xpos<0.15*player(curship)\v\w
						xpos = xpos - maxshipinc*2
					Else
						xpos = xpos - maxshipinc
					EndIf
				End If
			End If

			; keep window nicely positioned
			If ships(curship)\x-xpos>0.6*player(curship)\v\w Then
				If xpos < 336-player(curship)\v\w Then
					If ships(curship)\x-xpos>0.85*player(curship)\v\w
						xpos = xpos + maxshipinc*2
					Else
						xpos = xpos + maxshipinc
					EndIf
				End If
			End If



			; keep window nicely positioned
			If ships(curship)\y-ypos<0.4*player(curship)\v\h Then
				If ypos > 0 Then
					If ships(curship)\y-ypos<0.15*player(curship)\v\h
						ypos = ypos - maxshipinc*2
					Else
						ypos = ypos - maxshipinc
					EndIf
				End If
			End If

			; keep window nicely positioned
			If ships(curship)\y-ypos>0.6*player(curship)\v\h Then
				If ypos < 1008-player(curship)\v\h Then
					If ships(curship)\y-ypos>0.85*player(curship)\v\h
						ypos = ypos + maxshipfallinc*2
					Else
						ypos = ypos + maxshipfallinc
					EndIf
				End If
			End If
			
			If ypos+player(curship)\v\h > 1008 Then ypos = 1008-player(curship)\v\h
			If xpos+player(curship)\v\w > 336 Then xpos = 336-player(curship)\v\w
								
			If KeyDown(keythrst(curship)) And player(curship)\fuel > 0 Then		;thrust!!
				shipstate = 1
				ships(curship)\xinc = ships(curship)\xinc + Float(rots(shipang)\x)*thrust#
				ships(curship)\yinc = ships(curship)\yinc + Float(rots(shipang)\y)*thrust#
				player(curship)\fuel = player(curship)\fuel-1
				anythrust=1
				If numpods > 0
					If lap(0,curship) = 0
						lap(0,curship) = MilliSecs()
					EndIf
				EndIf
			Else
				shipstate = 0
			EndIf
		
			DoDebug ( "Thrust"+curship, 2, ships(curship)\yinc )
			
			recs(curship)\x = xpos
			recs(curship)\y = ypos

			Gosub do_physics
			Gosub UnDrawShip


			If numpods > 0
				Gosub CheckPorts
			EndIf
			
			If KeyDown(keystop(curship))
				ships(curship)\xinc = 0
				ships(curship)\yinc = 0
			EndIf
			
			player(curship)\xpos = xpos
			player(curship)\ypos = ypos
			
			ships(curship)\ang = shipang
			ships(curship)\shipstate = shipstate
			If autorefuel=1 Then Gosub refuel

			.skip
		Next		; end of player loop

		SetDisplay(BackBuffer(), player(curship)\v)
		Gosub DoWater

		For curship=0 To numplayers-1
			xpos = player(curship)\xpos
			ypos = player(curship)\ypos
			shipang = ships(curship)\ang
			shipstate = ships(curship)\shipstate

			SetDisplay(BackBuffer(),player(curship)\v)
			HandleImage map, xpos, ypos
			DrawImage map, 0, 0
			Gosub drawship
		Next


		HandleImage map,0,0

		Gosub DoShooters
		Gosub animate_bops				
	
		DrawBullets()

		For curship = 0 To numplayers-1
			SetDisplay(BackBuffer(),player(curship)\v)
			HandleImage supermap, player(curship)\xpos, player(curship)\ypos
			DrawImage supermap, 0, 0
			Gosub DrawStats
		Next

		HandleImage supermap,0,0


		If KeyDown(60)
			fps = 1-fps
			fpsv\hide = 1-fps
		EndIf

																
		If fps = 1																
			SetDisplay(BackBuffer(), fpsv)
;			Color 0,100,0
;			Rect fpsv\x,fpsv\y,fpsv\w,fpsv\h
		
			lag = MilliSecs()-tchk
			tchk = MilliSecs()
			
			tlag=tlag+lag
			trounds#=trounds+1

			tav# = tlag/trounds
			Color 255,0,255
			DoText (0,0,"FPS:"+1000.0/tav,0)
			tcnt = tcnt + 1
			If tcnt = 150
				trounds = 0
				tlag=0
				tcnt=0
			EndIf
		EndIf
		
		DebugUpdate()

		If MouseX() <> moosex Or moosey <> MouseY()
			mousecount = 78
		EndIf
		
		If mousecount > 0
			vp = DoMouse(vp, moosex, moosey)
			mousecount = mousecount - 1
		EndIf
		
		moosex = MouseX()
		moosey = MouseY()

		Flip


		If KeyDown(67)
			sc = CreateImage (336,304)
			GrabImage sc,0,0
			SaveImage sc, "gp.bmp"
		EndIf

		If anythrust = 1
			If ImagesCollide(shipshape(ships(0)\ang,0),ships(0)\x, ships(0)\y, 0, shipshape(ships(1)\ang,0),ships(1)\x, ships(1)\y,0)
				txi# = ships(0)\xinc
				tyi# = ships(0)\yinc
				ships(0)\xinc = ships(1)\xinc
				ships(0)\yinc = ships(1)\yinc
			
				ships(1)\xinc = txi
				ships(1)\yinc = tyi
				anythrust = 0
				
				If Not SoundPlaying(sounds(shipcollide2)\s\s) Then PlaySound sounds(shipcollide2)\s\s
			EndIf
		EndIf
		
		If KeyDown(62)
			If DEBUG=True
				DebugOff()
			Else
				DebugOn()
			EndIf
		EndIf
		
		If KeyDown(63) Then	Gosub View1

		If KeyDown(64) Then	Gosub View2
		
		If KeyDown(65) Then	Gosub View3
					
		If KeyDown(59)
			Cls
			Gosub TypeMenu
			FreeImage(map):map=0
			Gosub LevelMenu
			Gosub InitPlayers

		EndIf
		SetBuffer BackBuffer()
;		ClsColor 255,0,255
		Cls
	Wend
Return



.CheckPorts
	pos = player(curship)\pos
	If RectsOverlap(pod(pos)\x,pod(pos)\y,pod(pos)\w,pod(pos)\h,ships(curship)\x,ships(curship)\y,1,1)
		pos = pos + 1

		If pos > numpods-1
			tlap = player(curship)\lap + 1
			lap(tlap,curship) = MilliSecs()
			player(curship)\lap = tlap
			pos = 0
		EndIf
		player(curship)\pos = pos
	EndIf
	
Return


.View1
	xoff = GraphicsWidth()/2 - 336/2

	player(0)\v\x = xoff
	player(0)\v\y = 0
	player(0)\v\w = 336
	player(0)\v\h = dispheight

	player(0)\vs\x = xoff+336
	player(0)\vs\y = 0
	player(0)\vs\w = 64
	player(0)\vs\h = 100

	player(1)\v\x = xoff
	player(1)\v\y = GraphicsHeight() - dispheight
	player(1)\v\w = 336
	player(1)\v\h = dispheight

	player(1)\vs\x = xoff+336
	player(1)\vs\y = GraphicsHeight() - dispheight
	player(1)\vs\w = 64
	player(1)\vs\h = 100
Return

.View2
	xoff = Max(GraphicsWidth()/2 - 338,0)
	player(1)\v\x = xoff
	player(1)\v\y = 0
	player(1)\v\w = Min(GraphicsWidth()/2 - 2,336)
	player(1)\v\h = GraphicsHeight()

	player(1)\vs\x = xoff
	player(1)\vs\y = 0
	player(1)\vs\w = 64
	player(1)\vs\h = 100

	xoff = GraphicsWidth()/2 + 2

	player(0)\v\x = xoff
	player(0)\v\y = 0
	player(0)\v\w = Min(GraphicsWidth()-xoff,336)
	player(0)\v\h = GraphicsHeight()

	player(0)\vs\x = xoff
	player(0)\vs\y = 0
	player(0)\vs\w = 64
	player(0)\vs\h = 100

Return


.View3
	xoff = Max(GraphicsWidth()/2 - 338,0)
	player(1)\v\x = xoff
	player(1)\v\y = 0
	player(1)\v\w = Min(GraphicsWidth()/2 - 2,336)
	player(1)\v\h = GraphicsHeight()

	player(1)\vs\x = 0
	player(1)\vs\y = 0
	player(1)\vs\w = 64
	player(1)\vs\h = 100

	xoff = GraphicsWidth()/2 + 2

	player(0)\v\x = xoff
	player(0)\v\y = 0
	player(0)\v\w = Min(GraphicsWidth()-xoff,336)
	player(0)\v\h = GraphicsHeight()

	player(0)\vs\x = GraphicsWidth()-64
	player(0)\vs\y = 0
	player(0)\vs\w = 64
	player(0)\vs\h = 100
Return


.DrawStats
	SetDisplay(BackBuffer(),player(curship)\vs)
	DrawImage panel,0,0

	Color 40,170,20
	Rect 7,16,52*player(curship)\shield / strtshield,3
	Rect 7,32,52*player(curship)\fuel / strtfuel,3
	Rect 7,48,52*player(curship)\ammo1 / weapon(ships(curship)\weap1)\load,3
	Rect 7,64,52*player(curship)\ammo2 / weapon(ships(curship)\weap2)\load,3


	tw# = 25.0
	Color 255,0,0
	Rect 34,52,tw*player(curship)\numweap1/weapon(ships(curship)\weap1)\atonce,1
	Rect 34,68,tw*player(curship)\numweap2/weapon(ships(curship)\weap2)\atonce,1

	DrawImage shipshape(numrots/4,curship*2),24,75
	DrawImage weapon(ships(curship)\weap2)\icon,10,70
	DrawImage weapon(ships(curship)\weap1)\icon,30,70
	
	Color 20,200,240
	Text 45,70,player(curship)\score
Return


.LoadSounds
	bigexp = DoSound("sfx/bigexp.wav")
	bingo = DoSound("sfx/bingo.wav")
	bullethit = DoSound("sfx/bullethit.wav")
	checkpoint = DoSound("sfx/checkpoint.wav")
;	firemissile = DoSound("sfx/firemissile.wav")
	shipcollide = DoSound("sfx/shipcollide.wav")
	shipcollide2 = DoSound("sfx/shipcollide2.wav")
	splash = DoSound("sfx/splash.wav")
	tingaling = DoSound("sfx/tingaling.wav")
	touchdown = DoSound("sfx/touchdown.wav")
Return


.LoadGFX
	Global mousep = LoadImage("gfx/mousepoint.bmp")
	Global mouser = LoadImage("gfx/mouseresize.bmp") : HandleImage mouser, 6, 6
	Global mousem = LoadImage("gfx/mousemove.bmp") : HandleImage mousem, 6, 6

	shipshape(0,0) = LoadImage ("gfx/gship.bmp")
	shipshape(0,1) = LoadImage ("gfx/gshipf.bmp")
	shipshape(0,2) = LoadImage ("gfx/gship2.bmp")
	shipshape(0,3) = LoadImage ("gfx/gship2f.bmp")
	
	shipshape(0,4) = LoadImage ("gfx/_gship.bmp")
	shipshape(0,5) = LoadImage ("gfx/_gshipf.bmp")
	
	shipshape(0,6) = LoadImage ("gfx/lpad.bmp")
	shipshape(0,7) = LoadImage ("gfx/rpad.bmp")


	For loop=0 To 20
		kaboomanim(loop) = LoadImage("gfx/bigexp_"+loop+".bmp")
		HandleImage kaboomanim(loop),13,13
	Next
	MaskImage kaboomanim(20),255,255,255


	MaskImage shipshape(0,4),255,255,255
	MaskImage shipshape(0,5),255,255,255

	HandleImage(shipshape(0,0), 3, 6)
	HandleImage(shipshape(0,1), 3, 6)
	HandleImage(shipshape(0,2), 3, 6)
	HandleImage(shipshape(0,3), 3, 6)
	HandleImage(shipshape(0,4), 3, 6)
	HandleImage(shipshape(0,5), 3, 6)
	HandleImage(shipshape(0,6), 3, 6)
	HandleImage(shipshape(0,7), 3, 6)

	shipshape(0,8) = CopyImage(shipshape(0,0))



	Global bullet = LoadImage("gfx/bullet.bmp")
	mask = LoadImage("gfx/mask.bmp")
	bulletexp1 = LoadImage("gfx/bulletexp1.bmp"):HandleImage bulletexp1, 1,2
	bulletexp2 = LoadImage("gfx/bulletexp2.bmp"):HandleImage bulletexp2, 1,2
	bulletexp3 = LoadImage("gfx/bulletexp3.bmp"):HandleImage bulletexp3, 1,2
	bulletmask = LoadImage("gfx/bulletmask.bmp"):HandleImage bulletmask, 1,2
	MaskImage bulletmask, 255, 255, 255
	bombmask = LoadImage("gfx/bombmask.bmp")
	
	MaskImage mask,255,255,255
	
	deg# = 360.0/numrots
	off = 90
	
	For loop = 1 To numrots
		ang# = 1.0/numrots
		For shiploop = 0 To 8
			shipshape(loop,shiploop) = CopyImage (shipshape(0,shiploop))
			If shiploop < 4 Then
				TFormFilter 1
			Else
				TFormFilter 0
			EndIf
			RotateImage(shipshape(loop,shiploop), deg*loop)
		Next

		Cls
		DrawImage shipshape(loop,0),GraphicsWidth()/2,GraphicsHeight()/2
	
		r.rot = New rot
		r\x = Cos(deg * loop - off)
		r\y = Sin(deg * loop - off)
		rots(loop) = r
	Next
	
	r = New rot
	r\x = 0: r\y = -1
	rots(0) = r
;	Gosub LoadLvlGFX
Return


Function CheckBit (byte, bit)
	Return (byte Sar bit) And 1
End Function


Function bits_to_number (bt1, bt2, bt3, bt4)
	value = value + bt1 * 1
	value = value + bt2 * 2
	value = value + bt3 * 4
	value = value + bt4 * 8

	Return value
End Function


Function LoadLvlGFX( lv.levtypes, slot )

	If lv\loaded = 1 Then Return


	SetBuffer FrontBuffer()

	lvlf = ReadFile ("gfx/"+lv\bitmap)
	fsize = FileSize ("gfx/"+lv\bitmap)

	count = 0
	While Not Eof( lvlf )
		byte = ReadByte(lvlf)
		dpckin(count, slot) = byte
		count = count + 1
	Wend

	CloseFile lvlf

	fsize = depack(fsize, slot)


	bt1 = dpckin(0,slot)
	bt2 = dpckin(1,slot)
	bt3 = dpckin(2,slot)
	bt4 = dpckin(3,slot)

 
	colpos = (fsize-32)

	tst$ = Chr$(bt1)+Chr$(bt2)+Chr$(bt3)+Chr$(bt4)
						
    For loop=0 To 15
	  	byte1 = dpckout(colpos,slot)
	  	colpos=colpos + 1

	  	byte2 = dpckout(colpos,slot)
	  	colpos=colpos + 1

	 	long = byte1*256+byte2
	
		If long<0 Then long=long+2^16
		If long>16384 Then long=long-16384
		If long>16384 Then long=long-16384

;Print "long:"+long

      	rpr1 = long/256

      	long=long-(rpr1*256)
      	rpg1 = (long/16)

      	long=long-(rpg1*16)
      	rpb1=long

		cols(loop,0) = rpr1*16
		cols(loop,1) = rpg1*16
		cols(loop,2) = rpb1*16
		
		DoDebug("RCol", loop+1, rpr1*16+":"+rpg1*16+":"+rpb1*16)
		
		GetTrueColour(rpr1*16, rpg1*16, rpb1*16)

		DoDebug("Colour", loop+1, ColorRed()+":"+ColorGreen()+":"+ColorBlue())
		

		mcols(loop,0,lv\shapeslot) = ColorRed()
		mcols(loop,1,lv\shapeslot) = ColorGreen()
		mcols(loop,2,lv\shapeslot) = ColorBlue()


		Plot 0,0
		
		cols(loop,0) = CreateImage(1,1)
		GrabImage cols(loop,0),0,0

		Color 0,0,0
		Plot 0,0

    Next

	pos = 7
	name$ = ""
	While dpckout(pos,slot) > 0
		name$=name$+Chr$(dpckout(pos,slot))
		pos = pos + 1
	Wend
;	Print "Level:"+name$
	
	pos = 29
	author$ = ""
	While dpckout(pos,slot) > 0
		author$=author$+Chr$(dpckout(pos,slot))
		pos = pos + 1
	Wend
;	Print "Author:"+author$


    numshapes=(fsize-54-32)/128
	lv\numshapes=numshapes

	off=54
	bpl_off = 2

	xpos = 0
	ypos = 0


	SetBuffer FrontBuffer()
		

	tmp = CreateImage(16,16)

	shapes(0,0,lv\shapeslot) = CreateImage(16,16)
	minshapes(0) = CreateImage(4,4)

	If tst$ = "BPCK" Then
		bpl_off = numshapes*32
		For shape_loop = 1 To numshapes - 1
			indust=0
			shp = CreateImage(16,16)
			SetBuffer ImageBuffer(shp)
			
			For yloop = 0 To 15
				For xloop = 0 To 1
					For bit_loop = 0 To 7
						bit1 = CheckBit(dpckout(off + bpl_off*0,slot), 7-bit_loop)
						bit2 = CheckBit(dpckout(off + bpl_off*1,slot), 7-bit_loop)
						bit3 = CheckBit(dpckout(off + bpl_off*2,slot), 7-bit_loop)
						bit4 = CheckBit(dpckout(off + bpl_off*3,slot), 7-bit_loop)

						pxl = bits_to_number(bit1, bit2, bit3, bit4)
;						Color cols(pxl,0,slot),cols(pxl,1,slot),cols(pxl,2,slot)
					
						If pxl >= 8
							SetBuffer ImageBuffer(tmp)
							DrawBlock cols(pxl,0),xpos+bit_loop,ypos
							indust=1
							SetBuffer ImageBuffer(shp)
						EndIf

						DrawBlock cols(pxl,0),xpos+bit_loop,ypos
					Next
					off = off + 1
					xpos = xpos + 8
				Next
				ypos = ypos + 1
				xpos = xpos-16
			Next
			ypos = ypos - 16
		
			If indust=1 Then
				If shapes(shape_loop,1,lv\shapeslot) <> 0 Then FreeImage(shapes(shape_loop,1,lv\shapeslot))
				shapes(shape_loop,1,lv\shapeslot) = tmp
				tmp = CreateImage(16,16)
			Else
				shapes(shape_loop,1,lv\shapeslot) = 0
			EndIf
			If shapes(shape_loop,0,lv\shapeslot) <> 0 Then FreeImage(shapes(shape_loop,0,lv\shapeslot))
			shapes(shape_loop,0,lv\shapeslot) = shp
		Next
	Else
		bpl_off = 2
		For shape_loop = 1 To numshapes - 1
			indust=0
			shp = CreateImage(16,16)
			SetBuffer ImageBuffer(shp)
			For yloop = 0 To 15
				For xloop = 0 To 1
					For bit_loop = 0 To 7
						bit1 = CheckBit(dpckout(off + bpl_off*0,slot), 7-bit_loop)
						bit2 = CheckBit(dpckout(off + bpl_off*1,slot), 7-bit_loop)
						bit3 = CheckBit(dpckout(off + bpl_off*2,slot), 7-bit_loop)
						bit4 = CheckBit(dpckout(off + bpl_off*3,slot), 7-bit_loop)

						pxl = bits_to_number(bit1, bit2, bit3, bit4)
;						Color cols(pxl,0,slot),cols(pxl,1,slot),cols(pxl,2,slot)
					
						If pxl >= 8
							SetBuffer ImageBuffer(tmp)
							DrawBlock cols(pxl,0),xpos+bit_loop,ypos
							indust=1
							SetBuffer ImageBuffer(shp)
						EndIf

						DrawBlock cols(pxl,0),xpos+bit_loop,ypos
					Next
					off = off + 1
					xpos = xpos + 8
				Next
				off = off + 6
				ypos = ypos + 1
				xpos = xpos-16
			Next
			ypos = ypos - 16
		
			If indust=1
				If shapes(shape_loop,1,lv\shapeslot) <> 0 Then FreeImage(shapes(shape_loop,1,lv\shapeslot))
				shapes(shape_loop,1,lv\shapeslot) = tmp
				tmp = CreateImage(16,16)
			Else
				shapes(shape_loop,1,lv\shapeslot) = 0
			EndIf
			If shapes(shape_loop,0,lv\shapeslot) <> 0 Then FreeImage(shapes(shape_loop,0,lv\shapeslot))
			shapes(shape_loop,0,lv\shapeslot) = shp		
		Next
	EndIf

;	For loop = 1 To numshapes-1
;		If minshapes(loop) <> 0 Then FreeImage(minshapes(loop))
;		
;		If shapes(loop,0,lv\shapeslot) <> 0 Then minshapes(loop) = CopyImage(shapes(loop,0,lv\shapeslot))
;		TFormFilter 1
;		ScaleImage(minshapes(loop),0.25,0.25)
;	Next
			
	For loop = 0 To 15
		FreeImage cols(loop,0)
	Next
	
	lv\loaded = 1
End Function


.drawship
	If numpods > 0
		Color 255,255,255
		If player(curship)\lap < 3
			DoText(ships(curship)\x-5-xpos, ships(curship)\y+5-ypos, player(curship)\pos+1,2)
		Else
			Text ships(curship)\x-5-xpos, ships(curship)\y+5-ypos, (lap(1,curship)-lap(0,curship))/1000.0
			Text ships(curship)\x-5-xpos, ships(curship)\y+15-ypos, (lap(2,curship)-lap(1,curship))/1000.0
			Text ships(curship)\x-5-xpos, ships(curship)\y+25-ypos, (lap(3,curship)-lap(2,curship))/1000.0
		EndIf		
	EndIf

	y0 = ships(curship)\y
	SetDisplay(BackBuffer(),player(curship)\v)
	DrawImage shipshape(shipang,shipstate+(curship*2)), ships(curship)\x-xpos, y0-ypos

	If numplayers=2 Then 	
		y1 = player(1-curship)\ypos
		If y0>y1 And y0<y1+player(1-curship)\v\h
			SetDisplay(BackBuffer(),player(1-curship)\v)
			DrawImage shipshape(shipang,shipstate+(curship*2)), ships(curship)\x-player(1-curship)\xpos, y0-y1
		EndIf
	EndIf


Return

.UnDrawShip
	SetBuffer ImageBuffer(map)
	DrawImage shipshape(shipang,4), ships(curship)\x, ships(curship)\y
Return


.fire_ammo2
	tship# = shipang
	shipang = (shipang-8+Rnd(numrots/8)) Mod numrots
	If shipang<0 Then shipang = shipang + numrots
	Gosub fire_bop
	shipang = tship
Return


.fire_bop

	a.airobj = New airobj
			
	x# = rots(shipang)\x
	y# = rots(shipang)\y

	a\x = ships(curship)\x+x#*9
	a\y = ships(curship)\y+y#*9
	
	a\ox = a\x
	a\oy = a\y
	
	a\xinc = ships(curship)\xinc+x#*1.5
	a\yinc = ships(curship)\yinc+y#*1.5
	
	a\player = curship

Return


.animate_bops
	y1 = player(0)\ypos
	y2 = player(1)\ypos
	
	SetBuffer ImageBuffer(map)

	
	For a=Each airobj
		
		wi.WeaponInstance = a\wi
		
		If a\dead > 0 Then
			If wi <> Null
				ex$	= wi\t\explosion
			Else
				ex$ = ""
			EndIf
			
			If ex$ = "big"
				If a\dead = 1 DoPlaySound bigexp:Gosub DoShockwave
				If a\dead < 20
					DrawImage kaboomanim(a\dead),a\x,a\y
				Else
					DrawImage kaboomanim(a\dead),a\x,a\y
					If wi\mounted=0
						player(a\player)\numweap1 = player(a\player)\numweap1 - 1
					Else
						player(a\player)\numweap2 = player(a\player)\numweap2 - 1
					EndIf
					Delete a
					Delete wi\t
					Delete wi
					Goto lo
				EndIf
			Else
				SetBuffer ImageBuffer(map)
				Select a\dead
					Case 1
						DrawImage bulletexp1,a\x,a\y
						DoPlaySound bullethit
				
					Case 3
						DrawImage bulletexp2,a\x,a\y
				
					Case 5
						DrawImage bulletexp3,a\x,a\y
				
					Case 7
						DrawImage bulletmask,a\x,a\y
						player(a\player)\numbullets = player(a\player)\numbullets - 1
						If wi <> Null
							If wi\mounted=0
								player(a\player)\numweap1 = player(a\player)\numweap1 - 1
							Else
								player(a\player)\numweap2 = player(a\player)\numweap2 - 1
							EndIf
							Delete wi\t
							Delete wi
						EndIf
						Delete a
						Goto lo
				End Select
			EndIf

			a\dead = a\dead + 1

			Goto lo
		EndIf

	
		; Do position increments		
		If a\antigrav = 0
			If wi <> Null
				If wi\t\power > 0
					If wi\t\guided > 0
						If round > wi\guidedframe + (75/wi\t\guided)
							wi\guidedframe = round
							ang# = (ATan2(ships(1-a\player)\x - a\x, a\y-ships(1-a\player)\y)+360) Mod 360
							
							ang = ang * (numrots/360.0)
;							If ang < 0 Then ang=ang+numrots
							If Abs(ang-a\pang) < numrots/2
								a\pang = (a\pang+a\pang+ang)/3
							Else
								a\pang = (a\pang+a\pang+ang+numrots)/3  Mod numrots
							EndIf
						EndIf
					EndIf
			

					xinc# = rots(a\pang)\x*wi\t\speed
					yinc# = rots(a\pang)\y*wi\t\speed
					wi\t\power = wi\t\power - 1
				Else
					xinc# = a\xinc ; * 0.995
					yinc# = a\yinc + gravity#*2
				EndIf
			Else
				xinc# = a\xinc ; * 0.995
				yinc# = a\yinc + gravity#*2
			EndIf
		Else
			xinc# = a\xinc
			yinc# = a\yinc
		EndIf
		
		If wi<>Null
			mspeed = wi\t\speed
		Else
			mspeed = 2
		EndIf
		
		If xinc# < -mspeed Then 
			xinc# = -mspeed
		ElseIf xinc# > mspeed Then 
			xinc# = mspeed
		EndIf
	
		If yinc# < -mspeed Then 
			yinc# = -mspeed
		ElseIf yinc# > mspeed Then 
			yinc# = mspeed
		EndIf

		a\xinc = xinc#
		a\yinc = yinc#
		a\x = a\x + xinc#
		a\y = a\y + yinc#
		
		If a\x<0 Then a\x= a\ox:a\dead=1:Goto lo
		If a\y<0 Then a\y= a\oy:a\dead=1:Goto lo
		If a\y>1008 Then a\y= a\oy:a\dead=1:Goto lo
		If a\x>336 Then a\x = a\ox:a\dead=1:Goto lo
		
		
		If wi<>Null
			If wi\t\rotate = "true"
				abc = ((ATan2(a\x-a\ox, a\oy-a\y)/360.0) * numrots) Mod numrots
				If abc<0 Then abc=abc+numrots
				
				abc = (abc+a\oang)/2
				a\oang = abc

				
				wi\t\image = ammorot( wi\t\id, abc )
;				Stop
			EndIf
		EndIf
		
					
		a\ox = a\x
		a\oy = a\y				

		If wi<>Null
			img = wi\t\image
		Else
			img = bullet
		EndIf
						
		For curship = 0 To 1
			If ImagesCollide(img, a\x, a\y, 0, shipshape(shipang,0), ships(curship)\x, ships(curship)\y, 0)
				If wi<>Null
					If wi\t\explodetarget = "true"
						; Give a few rounds of 'grace' to avoid player killing themselves ;-)
						If round - wi\startframe > 5
							a\dead=1
							Goto lo
						EndIf
					Else
						player(curship)\shield = player(curship)\shield - 1
					EndIf
				Else
					player(curship)\shield = player(curship)\shield - 1
				EndIf

				
				If player(curship)\shield < 0 Then Gosub KillPlayer
			EndIf
		Next

		Color 0,0,0
		hit = 0
		If a\antigrav = 0 Then
			If ImagesCollide(bullet, a\x, a\y, 0, map, 0, 0, 0 ) Or ImagesCollide(bullet, a\x, a\y, 0, supermap, 0, 0, 0 ) Then
				hit = 1
			EndIf
		ElseIf a\antigrav = 1
			If a\x = a\s\ex And a\y = a\s\ey Then
				If ImagesCollide(bullet, a\x, a\y, 0, map, 0, 0, 0 ) Or ImagesCollide(bullet, a\x, a\y, 0, supermap, 0, 0, 0 ) Then
					player(a\player)\numbullets = player(a\player)\numbullets - 1
					Delete a
					Goto lo
				Else
					a\s\ex = a\x + a\xinc
					a\s\ey = a\y + a\yinc
				EndIf
			EndIf
		EndIf
	
		If hit=1
			a\dead = 1
			Goto lo
		EndIf

		.lo
	Next
Return


.DoShockwave
	damdist=80
	shockdist=120
	For l=0 To numplayers-1
		dx=(ships(l)\x-a\x)
		dy=(a\y-ships(l)\y)
		dist = Sqr(dx^2+dy^2)
		If dist < damdist
			If dist = 0 Then dist = 1
			dam# = Float(damdist/dist)
			If player(l)\shield - dam < 0
				curship=l
				Gosub KillPlayer
			Else
				player(l)\shield = player(l)\shield - dam
			EndIf
		EndIf
		
		If dist < shockdist
			pow# = 1-Float(dist/shockdist)
			mrot# = (ATan2(dx,dy) * (numrots/360.0)) Mod numrots
			If mrot<0 Then mrot=mrot+numrots
			ships(l)\xinc = ships(l)\xinc+rots(mrot)\x*pow
			ships(l)\yinc = ships(l)\yinc+rots(mrot)\y*pow
		EndIf
	Next
Return


.DoShooters
	For s.shooter=Each shooter
		s\timeout = s\timeout - 1
		If s\timeout = 0 And s\dead = 0
			If ImagesCollide (bulletexp1, s\mx, s\my, 0, map, 0, 0, 0) Or s\inv = 1
				a.airobj = New airobj

				a\xinc# = rots(s\dir*(numrots/4))\x
				a\yinc# = rots(s\dir*(numrots/4))\y
	
				a\x = s\x+a\xinc*2
				a\y = s\y+a\yinc*2
	
				If s\ex = s\x And s\ey = s\y Then
					s\ex = a\x + a\xinc
					s\ey = a\y + a\yinc
				EndIf
	
				a\ox = a\x
				a\oy = a\y
	
				a\player = 2
				a\antigrav = 1
				a\s = s
				s\timeout = s\timer
			Else
				s\dead = 1
			EndIf
		EndIf
	Next
Return



Function DrawBullets()
	x1 = player(0)\xpos
	x2 = player(1)\xpos

	y1 = player(0)\ypos
	y2 = player(1)\ypos

;	For wi.WeaponInstance=Each WeaponInstance
	For a.airobj = Each airobj
		If a\dead = 0
			If a\wi <> Null
				image = a\wi\t\image
			Else
				image = bullet
			EndIf
			
			If a\y > y1 And a\y < y1 + player(0)\v\h
				SetDisplay(BackBuffer(),player(0)\v)
				DrawImage image, a\x-x1, a\y-y1
			EndIf
			
			If a\y > y2 And a\y < y2 + player(1)\v\h
				SetDisplay(BackBuffer(),player(1)\v)
				DrawImage image, a\x-x2, a\y-y2
			EndIf
		EndIf
	Next

;	For a.airobj=Each airobj
;		If a\dead = 0
;			If a\y > y1 And a\y < y1 + player(0)\v\h
;				SetDisplay(BackBuffer(),player(0)\v)
;				DrawImage bullet, a\x-x1, a\y-y1
;			EndIf
;			
;			If numplayers = 2 
;				If a\y > y2 And a\y < y2 + player(1)\v\h
;					SetDisplay(BackBuffer(),player(1)\v)
;					DrawImage bullet, a\x-x2, a\y-y2
;				EndIf
;			EndIf
;		EndIf
;	Next
End Function



.do_physics

	DoDebug ( "Thrust"+curship, 3, ships(curship)\yinc )

	SetBuffer FrontBuffer()

	If ships(curship)\landed = 0 Then
		If ships(curship)\y < water_s
			yinc# = ships(curship)\yinc + gravity#
			If player(curship)\underwater = 1 Then yinc = yinc * 0.5
			player(curship)\underwater = 0
		Else
			If player(curship)\underwater = 0 Then DoPlaySound splash
			yinc# = ships(curship)\yinc - gravity#*1.3
			player(curship)\underwater = 1
		EndIf
	Else
		yinc# = ships(curship)\yinc
;		ships(curship)\landed = 0
	EndIf
	
	
	xinc# = ships(curship)\xinc  * 0.99

	For m.magnet = Each magnet
		If RectsOverlap(ships(curship)\x, ships(curship)\y, 1, 1, m\x, m\y, m\w, m\h)
			If m\dir = 0
				xinc# = xinc# - ( thrust * 1.5 * ((m\x + m\w - ships(curship)\x) / 64.0) )
			Else
				xinc# = xinc# + ( thrust * 1.5 * ((ships(curship)\x - m\x) / 64.0) )
			EndIf
		Else
;		SetBuffer ImageBuffer(map)
;			Color 255,0,200
;			Rect m\x,m\y,m\w,m\h
;		SetBuffer FrontBuffer()
		EndIf
	Next
	

	
	If player(curship)\underwater = 0
		If xinc# < -maxshipinc# Then 
			xinc# = -maxshipinc#
		ElseIf xinc# > maxshipinc# Then 
			xinc# = maxshipinc#
		EndIf
	
		If yinc# < -maxshipinc# Then 
			yinc# = -maxshipinc#
		ElseIf ships(curship)\shipstate = 1 
			If yinc# > maxshipfallinc#
				yinc# = maxshipfallinc#
			EndIf
		ElseIf yinc# > maxshipinc#
			yinc = yinc * 0.99
		EndIf
	Else
		If xinc# < -maxshipinc#/2 Then 
			xinc# = -maxshipinc#/2
		ElseIf xinc# > maxshipinc#/2 Then 
			xinc# = maxshipinc#/2
		EndIf
	
		If yinc# < -maxshipinc#/2 Then 
			yinc# = -maxshipinc#/2
		ElseIf yinc# > maxshipfallinc#/2 Then 
			yinc# = maxshipfallinc#/2
		EndIf
	EndIf

	DoDebug ( "Thrust"+curship, 8, ships(curship)\yinc )
	DoDebug ( "Thrust"+curship, 9, ships(curship)\y )

	ships(curship)\xinc = xinc#
	ships(curship)\yinc = yinc# ;+ (Abs(yinc#) *gravity#)
	ships(curship)\x = ships(curship)\x + ships(curship)\xinc
	ships(curship)\y = ships(curship)\y + ships(curship)\yinc

	DoDebug ( "Thrust"+curship, 11, ships(curship)\yinc )
	DoDebug ( "Thrust"+curship, 12, ships(curship)\y )

	If ships(curship)\x < 0 Then
		ships(curship)\x = 0
		ships(curship)\xinc = -ships(curship)\xinc * 0.5
	EndIf
	
	If ships(curship)\x > 336 Then
		ships(curship)\x = 336
		ships(curship)\xinc = -ships(curship)\xinc * 0.5
	EndIf
	
	If ships(curship)\y < 0 Then
		ships(curship)\y = 0
	EndIf


	If ImagesCollide( shipshape(shipang,8) , ships(curship)\x, ships(curship)\y, 0, supermap, 0, 0, 0 )				
		If ImagesCollide( shipshape(shipang,8) , ships(curship)\x-ships(curship)\xinc, ships(curship)\y, 0, supermap, 0, 0, 0 )
			ships(curship)\y = ships(curship)\y - ships(curship)\yinc
			ships(curship)\yinc = -ships(curship)\yinc*0.3
			player(curship)\shield = player(curship)\shield - 8
			ships(curship)\xinc = ships(curship)\xinc * 0.8
		ElseIf ImagesCollide( shipshape(shipang,8) , ships(curship)\x, ships(curship)\y-ships(curship)\yinc, 0, supermap, 0, 0, 0 )
			ships(curship)\x = ships(curship)\x - ships(curship)\xinc
			ships(curship)\xinc = -ships(curship)\xinc*0.3
			player(curship)\shield = player(curship)\shield - 8
			ships(curship)\yinc = ships(curship)\yinc * 0.8
		Else
			player(curship)\shield = player(curship)\shield - 1
		EndIf
	ElseIf ImagesCollide( shipshape(shipang,8) , ships(curship)\x, ships(curship)\y, 0, map, 0, 0, 0 )
		ships(curship)\xinc = ships(curship)\xinc * 0.8
		ships(curship)\yinc = ships(curship)\yinc * 0.8
		player(curship)\shield = player(curship)\shield - 1
	EndIf

	If player(curship)\shield < 0 Then Gosub KillPlayer

	
	xc# = ships(curship)\xinc
	yc# = ships(curship)\yinc
	shipvect# = (ATan2(xc + xc*shipstate, yc+yc*shipstate)) Mod 360
	shiprang = deg*ships(curship)\ang




;	GetColor(nosex, nosey)
;	If ColorRed() = 0 And ColorGreen() = 0 And ColorBlue() = 0 Then
;		nosehit=0
;	Else
;		nosehit=1
;	EndIf

;	Color 255,255,255:Plot nosex,nosey



;	GetColor(leftx, lefty)
;	If ColorRed() = 0 And ColorGreen() = 0 And ColorBlue() = 0 Then
;		lefthit=0
;	Else
;		lefthit=1
;		If ColorRed()=lp1 And ColorGreen()=lp2 And ColorBlue()=lp2
;			pad = 1
;		Else
;			pad = 0
;		EndIf
;		ships(curship)\landed = 1
;	EndIf

	If ImagesCollide (shipshape(shipang,6),ships(curship)\x,ships(curship)\y,0, map,0,0,0) Or ImagesCollide(shipshape(shipang,6),ships(curship)\x,ships(curship)\y,0, supermap,0,0,0)
		lefthit=1
		shipcollide=0
	ElseIf ImagesCollide (shipshape(shipang,6),ships(curship)\x,ships(curship)\y,0, shipshape(ships(1-curship)\ang,0),ships(1-curship)\x,ships(1-curship)\y,0)
		lefthit=1
		shipcollide=1
	Else
		lefthit=0
	EndIf

	If ImagesCollide (shipshape(shipang,7),ships(curship)\x,ships(curship)\y,0, map,0,0,0) Or ImagesCollide(shipshape(shipang,7),ships(curship)\x,ships(curship)\y,0, supermap,0,0,0)
		righthit=1
		shipcollide=0
	ElseIf ImagesCollide (shipshape(shipang,7),ships(curship)\x,ships(curship)\y,0, shipshape(ships(1-curship)\ang,0),ships(1-curship)\x,ships(1-curship)\y,0)
		righthit=1
		shipcollide=1
	Else
		righthit=0
	EndIf
	
	ships(curship)\onpad=0

	If lefthit = 1 And righthit	= 1 And shipcollide=0

		If (player(curship)\underwater=0 And (shiprang<70 Or shiprang>290)) Or (player(curship)\underwater=1 And (shiprang>110 And shiprang<250))
			If Abs(ships(curship)\yinc) > 0.3 Or shipstate = 1 Then
				ships(curship)\xinc = ships(curship)\xinc * 0.7
				ships(curship)\yinc = -ships(curship)\yinc * 0.5
			Else
				ships(curship)\xinc = 0
				ships(curship)\yinc = 0
				
				ships(curship)\x = Int(ships(curship)\x)
				ships(curship)\y = Int(ships(curship)\y)
				
				landang# = (shipang + numrots/2) Mod numrots
				
				If player(curship)\underwater=0 Then lmod = 3 Else lmod=4
				
				landx = ships(curship)\x+rots(landang)\x*lmod
				landy = ships(curship)\y+rots(landang)\y*lmod

				If curship=0 Then DoDebug ( "Landing", 1, mcols(12,0,lv\shapeslot)+":"+mcols(12,1,lv\shapeslot)+":"+mcols(12,2,lv\shapeslot))

				SetBuffer ImageBuffer(supermap)
				GetColor(landx, landy)

				If curship=0 Then DoDebug ( "Landing", 2, ColorRed()+":"+ColorGreen()+":"+ColorBlue())
				If curship=0 Then DoDebug("Colour", 17, ColorRed()+":"+ColorGreen()+":"+ColorBlue())
			
				If ColorRed()=mcols(12,0,lv\shapeslot) And ColorGreen()=mcols(12,1,lv\shapeslot) And ColorBlue()=mcols(12,2,lv\shapeslot)
					If ships(curship)\landed = 0 Then DoPlaySound touchdown
					ships(curship)\onpad = 1
					Gosub refuel
				Else
					ships(curship)\onpad = 0
				EndIf

				ships(curship)\landed = 1
;				Plot landx-player(curship)\xpos,landy-player(curship)\ypos


			EndIf
		EndIf
	Else
		ships(curship)\landed=0
		If lefthit = 1
			If shipvect > (shipangmod*((shipang+numrots*0.7) Mod numrots))
				shipang = (shipang + numrots-amodf) Mod numrots
			Else
				shipang = (shipang + amodf) Mod numrots
			EndIf
			If shipcollide=0
				ships(curship)\xinc = ships(curship)\xinc * 0.8
				ships(curship)\yinc = ships(curship)\yinc * 0.8
			EndIf
		ElseIf righthit = 1
			If shipvect < (shipangmod*((shipang+numrots*0.3) Mod numrots))
				shipang = (shipang + numrots-amodf) Mod numrots
			Else
				shipang = (shipang + amodf) Mod numrots
			EndIf
			If shipcollide=0
				ships(curship)\xinc = ships(curship)\xinc * 0.8
				ships(curship)\yinc = ships(curship)\yinc * 0.8
			EndIf
		EndIf
		
	EndIf
	

	If Abs(ships(curship)\xinc) < 0.005 Then ships(curship)\xinc = 0
	If Abs(ships(curship)\yinc) < 0.005 Then ships(curship)\yinc = 0

	DoDebug ( "Thrust"+curship, 6, ships(curship)\yinc )
																		
	If ships(curship)\y > 1008 Then
		ships(curship)\y = 1004
		ships(curship)\yinc = 0
;		ships(curship)\yinc = -ships(curship)\yinc * 0.5
	EndIf
Return


.refuel

	If softplay=1 And ships(curship)\onpad = 0
		If player(curship)\shield<strtshield Then player(curship)\shield=player(curship)\shield+shieldinc/5

		Return
	EndIf		

	If player(curship)\shield<strtshield Then player(curship)\shield=player(curship)\shield+shieldinc


	If player(curship)\fuel<strtfuel Then player(curship)\fuel=player(curship)\fuel+fuelinc
	
	If player(curship)\ammo1<weapon(ships(curship)\weap1)\load
;		If player(curship)\reloadammo1 < round - 75/(weapon(ships(curship)\weap1)\loadtime)
			player(curship)\ammo1=player(curship)\ammo1+weapon(ships(curship)\weap1)\loadtime/75.0
;			player(curship)\reloadammo1 = round
;		EndIf
	EndIf

	If player(curship)\ammo2<weapon(ships(curship)\weap2)\load
;		If player(curship)\reloadammo2 < round - 75/(weapon(ships(curship)\weap2)\loadtime)
			player(curship)\ammo2=player(curship)\ammo2+weapon(ships(curship)\weap2)\loadtime/75.0
;			player(curship)\reloadammo2 = round
;		EndIf
	EndIf
Return


.ReadTypes
	SetBuffer FrontBuffer()
;EndGraphics
	lvcount=0
	gdir=ReadDir("types")
	skip=0
	Repeat
		f$=NextFile$( gdir )
		If f$="" Then Exit
		t=FileType( "types/"+f$ )
;		f$=LSet$( rf$,32 )
;		Print f$+" "+rf$
		If t=1 And f$<>".." And f$<>""
			lv.levtypes = New levtypes
			lv\ap = Left$(f$,2)

			levs(lvcount)=lv

			info = ReadFile("types/"+f$)
			
			bitmap$ = ""
			ch = ReadByte(info)
			While ch > 0
				bitmap=bitmap+Chr$(ch)
				ch = ReadByte(info)
			Wend
;Print "b:"+bitmap

			SeekFile(info, 43)		
			name$ = ""
			ch = ReadByte(info)
			While ch > 0
				name=name+Chr$(ch)
				ch = ReadByte(info)
			Wend

;Print "n:"+name

			SeekFile(info, 90)		
			title$ = ""
			ch = ReadByte(info)
			While ch <> 185 And ch > 0
				title=title+Chr$(ch)
				ch = ReadByte(info)
			Wend
			
;Print title
			lv\bitmap = bitmap
			lv\name = name
			lv\title = title
			lv\shapeslot = lvcount
												
;			Text 10,30+lvcount*20, lv\name
																						
			CloseFile info
			
;			Color 255,255,255
			lvcount = lvcount + 1
		EndIf

	Forever
;	numlevtypes = tcount
	CloseDir gdir

Return


.TypeMenu
	SetBuffer FrontBuffer()
	Cls
	lev = 0
	Color 0,200,0
	DoBigTextLined(97,20,"Gravity Power 2  (Muchly beta v0.6)",4)
	SetBuffer FrontBuffer()
	

	DoText(100,35,quotes(Rnd(qcnt)),7)
	
	lvcount = 0
	For lv.levtypes = Each levtypes
		DoText(110, 50+lvcount*10, lv\name, 2)
		lvcount = lvcount + 1
	Next
	
	lv = First levtypes
	
	kdly=0
	pos = 0
	curship=0
	Repeat
		curship = 1-curship
		
		If kdly > 0 Then kdly = kdly - 1 Else kdly = 0
		
		If kdly = 0 
			If KeyDown(keyup(curship)) And pos > 0
				pos = pos - 1
				lv = levs(pos)
				kdly = 5
			ElseIf KeyDown(keydwn(curship)) And pos < lvcount-1
				pos = pos + 1
				lv = levs(pos)
				kdly = 5
			EndIf
		EndIf

;Text 100,100,kdly
	
		VWait
		Color 0,0,0
		Rect 90,50,17,300
		Color 255,255,255
		DrawImage shipshape(numrots/4,0),100,50+pos*10+3
		If KeyDown(1) Then End
	Until KeyDown(28)=1 Or KeyDown(57)=1 Or KeyDown(keythrst(curship))

;	dir$="gfx/"+levtype(lev)\ap+"/"
;	dir$="gfx/GR/"
;	lp1=levtype(lev)\lp1
;	lp2=levtype(lev)\lp2
;	lp3=levtype(lev)\lp3
	
;	numshapes=levtype(lev)\numshapes
	
	LoadLvlGFX(lv, 1)
	
	SetBuffer BackBuffer()
Return

.ReadLevels
	levcount = 0
	Repeat
		f$=NextFile$( gdir )
		If f$="" Then Exit
		t=FileType( "levels/"+f$ )

		If t=1
			If Left$(f$,2)=lv\ap Then
				levelnames(levcount,lv\shapeslot)=f$
				levcount=levcount+1
			EndIf
		EndIf
	Forever
	CloseDir gdir
Return


.LevelMenu
	gdir=ReadDir("levels")
	
	If levelnames(0,lv\shapeslot) <> ""
		levcount = lv\numlevels
	Else
		Gosub ReadLevels
		lv\numlevels = levcount
	EndIf
	
	levt = 0
	
	SetBuffer BackBuffer()
	Cls
	
	SetBuffer FrontBuffer()
	lev = 0
	Cls
	
	
	
	olvname$=""
	
	curply=0
	yoff=3
	kdly=3
	Repeat
		curply = 1-curply
		If kdly > 0 Then kdly=kdly - 1 Else kdly = 0
		
		If kdly = 0
			If KeyDown(keyup(curply))
				If lev>0
					lev=lev-1
					scnt = 40
					kdly = 1
				EndIf
			ElseIf KeyDown(keydwn(curply)) Then
				If lev<levcount-1
					lev=lev+1
					scnt = 40
					kdly = 1
				EndIf
			EndIf
		EndIf



;		If KeyDown(keyright(curship)) Then

;		If KeyDown(keyleft(curship)) Then
;			
;		EndIf

		lvlname$="levels/"+levelnames(lev, lv\shapeslot)		
		If olvname$ <> lvlname$ And scnt=0
			If map <> 0 Then FreeImage(map):map=0
			
			Gosub LoadLevel
			
			map=PreviewDrawMap(lv)
;			ScaleImage(map,0.25,0.25)
			olvname$ = lvlname$
			
		EndIf
		
		SetBuffer BackBuffer()
		HandleImage map,0,yscr
		DrawBlock map,0,0

		yscr=yscr + yoff
		
		If yscr <= 0 Or yscr > (1008-GraphicsHeight())
			yoff = yoff * -1
		EndIf

;		VWait
;		SetBuffer FrontBuffer()
;		Color 0,0,0
;		Rect -5,20,15,300

		DoBigText (97,20,lv\title,1)

		amtl=(GraphicsHeight()-50)/10

		For loop = levt To levt+amtl
			If loop < levcount
				DoText (110,35+(loop-levt)*10, Mid$(levelnames(loop,lv\shapeslot), 3, Len(levelnames(loop,lv\shapeslot))-6),2)
			EndIf
		Next

;Text 30,150,"Pods:"+numpods+" x1:"+pod(0)\x+" y1:"+pod(0)\y+" w:"+pod(0)\w+" h:"+pod(0)\h
		If lev > levt+amtl-3 And levt+amtl < levcount-1
			levt = levt+1
		EndIf
		
		If lev < levt+3 And levt > 0
			levt = levt-1
		EndIf
		
		DrawImage shipshape(numrots/4,0),100,35+(lev-levt)*10+3
		If scnt> 0 Then scnt = scnt - 1
;		Text 20,250,"s:"+water_s+" e:"+water_e+" d:"+water_spd
		If KeyDown(1) Then End
		Flip
	Until KeyDown(28)=1 Or KeyDown(57)=1 Or KeyDown(keythrst(curply))
	
	scnt = 0
	
	lvlname$="levels/"+levelnames(lev,lv\shapeslot)
	
	Gosub LoadLevel
	SetBuffer BackBuffer()

Return


.LoadLevel
	lvlf = ReadFile (lvlname$)
	fsize = FileSize (lvlname$)

	count = 0
	While Not Eof( lvlf )
		byte = ReadByte(lvlf)
		dpckin(count, 0) = byte
		count = count + 1
	Wend

	depack(count, 0)

	numpods = 0

	; Check for race ports
	If dpckout(35,0) <> 1
		For loop = 0 To 3
			x1 = dpckout(36+loop*8,0)*256+dpckout(37+loop*8,0)
			y1 = dpckout(38+loop*8,0)*256+dpckout(39+loop*8,0)
			x2 = dpckout(40+loop*8,0)*256+dpckout(41+loop*8,0)
			y2 = dpckout(42+loop*8,0)*256+dpckout(43+loop*8,0)

			If x1 = 0 And x2 = 0 Then Exit
			
			pod(loop)\x = x1
			pod(loop)\y = y1
			pod(loop)\w = x2 - x1+16
			pod(loop)\h = y2 - y1+16
			numpods = numpods + 1
		Next
	EndIf


	;player 1
	bt1 = dpckout(94, 0)
	bt2 = dpckout(95, 0)
	ships(0)\x = bt1*256+bt2-108+4
	ships(0)\ox = ships(0)\x

	bt1 = dpckout(98, 0)
	bt2 = dpckout(99, 0)
	ships(0)\y = bt1*256+bt2+10
	ships(0)\oy = ships(1)\y

	;player 2
	bt1 = dpckout(102, 0)
	bt2 = dpckout(103, 0)
	ships(1)\x = bt1*256+bt2-108+4
	ships(1)\ox = ships(1)\x

	bt1 = dpckout(106, 0)
	bt2 = dpckout(107, 0)
	ships(1)\y = bt1*256+bt2+10
	ships(1)\oy = ships(1)\y
	
	For curship = 0 To 1		
		player(curship)\xpos = ships(curship)\x - player(curship)\v\w/2
		player(curship)\ypos = ships(curship)\y - player(curship)\v\h/2

		If player(curship)\xpos < 0 Then player(curship)\xpos = 0
		If player(curship)\xpos > 336-player(curship)\v\w Then player(curship)\xpos = 336-player(curship)\v\w

		
		If player(curship)\ypos < 0 Then player(curship)\ypos = 0
		If player(curship)\ypos > 1008-player(curship)\v\h Then player(curship)\ypos = 1008-player(curship)\v\h
	Next
	


	; water positions and speed
	bt1 = dpckout(110,0)
	bt2 = dpckout(111,0)
	water_s = bt1*256+bt2
	
	If water_s = 32768
		water_flg = False
	Else
		water_flg = True
	EndIf

	bt1 = dpckout(114,0)
	bt2 = dpckout(115,0)
	water_e = bt1*256+bt2
	
	If water_s=0 And water_e=0 Then water_s = 32768
	If water_s=32767 Then water_s = 32768
	
	bt1 = dpckout(118,0)
	bt2 = dpckout(119,0)
	bt3 = dpckout(120,0)
	bt4 = dpckout(121,0)
	
	water_spd = Abs((bt1*16777216)+(bt2*65536)+(bt3*256)+bt4)
	
	If water_spd > 0 Then water_spd = (50.0/78.0) * ((water_e-water_s) / water_spd)

	;remove any existing shooters
	For s.shooter = Each shooter
		Delete s
	Next
	
	For a.airobj = Each airobj
		Delete a
	Next
	
	Delete Each WeaponInstance
	
	For sloop = 0 To 9
		bt1 = dpckout(1453+sloop*12+0,0)
		bt2 = dpckout(1453+sloop*12+1,0)
		timer = (bt1*256)+bt2
		If timer = 0 Then Exit
		
		s.shooter = New shooter
		
		s\pretimer = timer * (78/50.0)  ; 78 frames vs 50 frames/sec
		s\timeout = s\pretimer
		
		
		bt1 = dpckout(1453+sloop*12+2,0)
		bt2 = dpckout(1453+sloop*12+3,0)
		mby = bt1*256+bt2
		my = mby/42
		mx = (mby - (my*42))*8
		mbx = dpckout(1453+sloop*12+4,0)
		s\mx = mx+(7-mbx)
		s\my = my
		
		s\inv = dpckout(1453+sloop*12+5,0)
		
		s\timer = dpckout(1453+sloop*12+6,0) * (78/50.0)
	
		s\dir = dpckout(1453+sloop*12+7,0)
		
		If s\dir = 2 Then
			s\dir = 3
		ElseIf s\dir = 3 Then
			s\dir = 2
		EndIf
		
		bt1 = dpckout(1453+sloop*12+8,0)
		bt2 = dpckout(1453+sloop*12+9,0)
;		s\x = (bt1*256)+bt2 - dpckout(1453+sloop*12+4,0) - 1
		s\x = (bt1*256)+bt2 ;- dpckout(1453+sloop*12+4,0)


		
		bt1 = dpckout(1453+sloop*12+10,0)
		bt2 = dpckout(1453+sloop*12+11,0) 
		s\y = (bt1*256)+bt2 ;- dpckout(1453+sloop*12+4,0)

		s\dx = s\x		
		s\dy = s\y
		
		s\ex = s\x
		s\ey = s\y
		
		
;		If dir = 0 Then s\y = s\y-10
	
	Next
	
	For m.magnet = Each magnet
		Delete m
	Next
	

	For mloop = 0 To 9
		x = dpckout(1573+mloop*2,0)
		y = dpckout(1574+mloop*2,0)
	
;		Stop

		If x = 0 Then Exit
		
		m.magnet = New magnet
				
		If CheckBit(x,7)
			m\w = 64
			m\x = (x-128)*16
			m\dir = 0
		Else
			m\w = 64
			m\x = ((x-1)*16) - m\w
			m\dir = 1
		EndIf
		
		m\y = ((y-1)*16)-2
		m\h = 20
	Next
	
	map = DrawMap(lv)
Return


.DoWater
;Color 255,225,195
;Text 30,150,"Water Start:"+water_s+" End:"+water_e+" Spd:"+water_spd
;Text 30,150,"Avail VidMem:"+AvailVidMem()+" Used: "+(startvid-AvailVidMem())

	If water_s = 32768 Then Return
;	If Abs(water_e - water_s) > 1008 Then Return
	
	If water_spd > 0 And water_s < water_e Then water_s = water_s + water_spd*10
	If water_spd < 0 And water_s > water_e Then water_s = water_s + water_spd*10
	
	Color 0,40,100
	
	For curship = 0 To numplayers-1
		SetDisplay(BackBuffer(),player(curship)\v)
		yoff = water_s-player(curship)\ypos
		If yoff<0 Then yoff=0
		Rect 0,yoff,336,player(curship)\v\h-yoff
	Next
Return


.KillPlayer
	If player(curship)\dead > 0 Then Return

	player(1-curship)\score =player(1-curship)\score+1

	ang = numrots/15
	For floop = 0 To 15
		a.airobj = New airobj
			
		x# = rots(ang*floop)\x
		y# = rots(ang*floop)\y

		a\x = ships(curship)\x+x#*9
		a\y = ships(curship)\y+y#*9
	
;		a\ox = a\x
;		a\oy = a\y
	
		a\xinc = ships(curship)\xinc+x#
		a\yinc = ships(curship)\yinc+y#
	
		a\player = 2
	Next

	player(curship)\dead=70

	DoPlaySound bigexp
	DoPlaySound bingo

	bt1 = dpckout(94+curship*8, 0)
	bt2 = dpckout(95+curship*8, 0)
	ships(curship)\x = bt1*256+bt2-108+4
	ships(curship)\ox = ships(0)\x

	bt1 = dpckout(98+curship*8, 0)
	bt2 = dpckout(99+curship*8, 0)
	ships(curship)\y = bt1*256+bt2+10
	ships(curship)\oy = ships(1)\y
	
	xpos = ships(curship)\x - player(curship)\v\w/2
	ypos = ships(curship)\y - player(curship)\v\h/2
	player(curship)\xpos = xpos
	player(curship)\ypos = ypos

	If player(curship)\xpos < 0 Then player(curship)\xpos = 0
	If player(curship)\xpos > 336-player(curship)\v\w Then player(curship)\xpos = 336-player(curship)\v\w

		
	If player(curship)\ypos < 0 Then player(curship)\ypos = 0
	If player(curship)\ypos > 1008-player(curship)\v\h Then player(curship)\ypos = 1008-player(curship)\v\h

	xpos = player(curship)\xpos
	ypos = player(curship)\ypos
					
	shipang = 0
	shipstate = 0

	i = curship
	Gosub InitPlayer
Return


Function DrawMap(lv.levtypes)
	map = CreateImage(336,1008)

	SetBuffer ImageBuffer(supermap)
	Cls
	
	SetBuffer ImageBuffer(map)

	VWait

	For yloop = 0 To 62
		For xloop = 0 To 20
			num = dpckout(130+xloop+yloop*21, 0)
			If num <= lv\numshapes-1 Then
				DrawBlock shapes(num,0,lv\shapeslot),xloop*16, yloop*16
				If shapes(num,1,lv\shapeslot) <> 0 Then
					SetBuffer ImageBuffer(supermap)
					DrawBlock shapes(num,1,lv\shapeslot),xloop*16, yloop*16
					SetBuffer ImageBuffer(map)
				EndIf
			Else
				DrawBlock shapes(0,0,lv\shapeslot),xloop*16, yloop*16
			EndIf
		Next
	Next

;	For s.shooter = Each shooter
;		Color 250,10,210
;		Plot s\mx,s\my
;	Next
			
	Return map
End Function


Function PreviewDrawMap(lv.levtypes)
	map = CreateImage(336,1008)
	
	SetBuffer ImageBuffer(map)
	Cls

	
	If water_s <> 32768
		If water_s > water_e
			If water_spd <> 0
				Color 0,30,75
				Rect 0,water_e,336,1008
			EndIf
			Color 0,40,100
			Rect 0,water_s,336,1008
		Else
			Color 0,40,100
			Rect 0,water_s,336,1008
			If water_spd <> 0
				Color 0,30,75
				Rect 0,water_e,336,1008
			EndIf
		EndIf
	EndIf

	If numpods > 0
		For loop = 0 To numpods-1
			Color 200,40,100
;			Rect pod(loop)\x,pod(loop)\y,pod(loop)\w,pod(loop)\h
		Next
	EndIf	

;	For s.shooter = Each shooter
;		Color 250,10,210
;		Plot s\mx,s\my
;	Next


	For yloop = 0 To 62
		For xloop = 0 To 20
			num = dpckout(130+xloop+yloop*21, 0)
			If num <= lv\numshapes-1 Then
				DrawImage shapes(num,0,lv\shapeslot),xloop*16, yloop*16
			Else
				DrawImage shapes(0,0,lv\shapeslot),xloop*16, yloop*16
			EndIf
		Next
	Next

	DrawImage shipshape(0,0), ships(0)\x, ships(0)\y
	DrawImage shipshape(0,2), ships(1)\x, ships(1)\y
	


	For s.shooter = Each shooter
		Color 10,10,10
		Plot s\dx,s\dy
	Next
			
	Return map
End Function


Function MiniDrawMap(lv.levtypes)
	minimap = CreateImage(84,252)

	SetBuffer ImageBuffer(minimap)

	For yloop = 0 To 62
		For xloop = 0 To 20
			num = dpckout(130+xloop+yloop*21, 0)
			If num <= lv\numshapes-1 Then
				DrawBlock minshapes(num),xloop*4, yloop*4
			Else
				DrawBlock minshapes(0),xloop*4, yloop*4
			EndIf
		Next
	Next

	Return minimap
End Function


Function Draw(mx, my)
	Stop	; deprecated
	DrawArea(mx, my, 0, 0, 336, player(curship)\v\h,0)
End Function


Function DrawArea(mx, my, dx, dy, dwidth, dheight, yoff)
	Stop	; deprecated
	ty=(my+dy)/16
	sy=dy-((dy+my) Mod 16)
	While sy<=(dy+dheight)
		ty=ty Mod 63
		tx=(mx+dx)/16
		sx=dx-((dx+mx) Mod 16)
		While sx<(dx+dwidth)
			num = dpckout(130+tx+ty*21, 0)
			If num <= numshapes-1 Then
				DrawBlock shapes(num,0,shapeslot),sx,sy+yoff
			Else
				DrawBlock shapes(0,0,shapeslot),sx,sy+yoff
			EndIf
			tx=tx+1:sx=sx+16
		Wend
		ty=ty+1:sy=sy+16
	Wend
End Function


Function gf(cnt,slot)
	num=dpckin(cnt,slot)
	Return num
End Function


Function depack(size, slot)
	bt1 = dpckin(0,slot)
	bt2 = dpckin(1,slot)
	bt3 = dpckin(2,slot)
	bt4 = dpckin(3,slot)

	tst$ = Chr$(bt1)+Chr$(bt2)+Chr$(bt3)+Chr$(bt4)
	
	If tst$ <> "BPCK"
;		Print "Not Compressed.."
		For loop = 0 To size
			dpckout(loop, slot) = dpckin(loop, slot)
		Next
		Return size
	EndIf
	
  	bit8=dpckin(4,slot)
  	bit16=dpckin(5,slot)

  	If bit8<0 Then bit8=bit8+256
  	If bit16<0 Then bit16=bit16+256


  	bt1=dpckin(10,slot)
  	bt2=dpckin(11,slot)

  	If bt1<0 Then bt1=bt1+256
  	If bt2<0 Then bt2=bt2+256

  grot_size=(bt1*256)+bt2
;Print "grot:"+grot_size
  
;  Dim gfx(grot_size)

    char=0
    chr_rpt=0

    cnt=12
    decount=0

    While decount<grot_size
      num = gf(cnt, slot):cnt=cnt+1
      If num<0 Then num=num+256

      If num=bit8 Or num=bit16

        If num=bit8
          num = gf(cnt, slot):cnt=cnt+1
          If num<0 Then num=num+256
          ch_rpt=num
          num = gf(cnt, slot):cnt=cnt+1
          If num<0 Then num=num+256

          char=num
		  
;Print "ch:"+ch_rpt		
          For loop=1 To ch_rpt
            dpckout(decount,slot)=char
            decount=decount+1
          Next
        End If

        If num=bit16
          num = gf(cnt, slot):cnt=cnt+1
          If num<0 Then num=num+256
          bt1=num
          num = gf(cnt, slot):cnt=cnt+1
          If num<0 Then num=num+256
          bt2=num
          ch_rpt=(bt1*256)+bt2
          num = gf(cnt, slot):cnt=cnt+1
;Print "ch16:"+ch_rpt

          If num<0 Then num=num+256
          char=num
		  For loop=1 To ch_rpt
            dpckout(decount,slot)=char
            decount=decount+1
          Next
        End If

      Else
        dpckout(decount,slot)=num
        decount=decount+1
      End If
    Wend


	Return decount
End Function


Function DoFont()
	gpfont = LoadImage ("gfx/gpfont.bmp")

	fil = ReadFile("gfx/gp.exe")
	SeekFile(fil,177770)
	For loop = 0 To 93
		charwidth(loop)=ReadByte(fil)
	Next
	CloseFile fil

	Return

	; don't need to read amiga exe just now...
	For loop = 0 To 7
		charcol(loop,0) = 100+Rnd(155)
		charcol(loop,1) = 100+Rnd(155)
		charcol(loop,2) = 100+Rnd(155)
	Next

	charcol(0,0)=000:	charcol(0,1)=000:	charcol(0,2)=000
	charcol(1,0)=255:	charcol(1,1)=000:	charcol(1,2)=000	
	charcol(2,0)=000:	charcol(2,1)=255:	charcol(2,2)=000
	charcol(3,0)=000:	charcol(3,1)=000:	charcol(3,2)=255
	charcol(4,0)=000:	charcol(4,1)=255:	charcol(4,2)=255
	charcol(5,0)=255:	charcol(5,1)=000:	charcol(5,2)=255
	charcol(6,0)=255:	charcol(6,1)=255:	charcol(6,2)=000
	charcol(7,0)=255:	charcol(7,1)=255:	charcol(7,2)=255

	fil = ReadFile("gfx/gp.exe")
	SeekFile(fil,176520)

	gpfont = CreateImage(800,64)
	SetBuffer ImageBuffer(gpfont)
	
	For charloop = 0 To 93
		For lineloop = 0 To 7
			byte = ReadByte(fil)
			For loop = 0 To 7
				If CheckBit(byte, 7-loop)
					For coloop = 0 To 7	
						Color charcol(coloop,0),charcol(coloop,1),charcol(coloop,2)
						Plot charloop*8+loop,lineloop+coloop*8
					Next
				EndIf
			Next
			ypos=ypos+1
		Next

;		For coloop = 0 To 7
;			charfont(charloop,coloop) = CreateImage(8,8)
;			GrabImage charfont(charloop,coloop), coloop*8,0
;		Next
	Next

	SeekFile(fil,177770)
	For loop = 0 To 93
		charwidth(loop)=ReadByte(fil)
	Next

	CloseFile(fil)
	
	SetBuffer FrontBuffer()
	SaveImage gpfont,"gfx/gpfont.bmp"
End Function


Function DoBigTextLined(x,y,txt$,colour)
	buf = GraphicsBuffer()
	tmpimage = CreateImage(Len(txt)*8,8)
	SetBuffer ImageBuffer(tmpimage)

	DoText(0,0,txt,colour)
	SetBuffer buf
	ScaleImage tmpimage,1,2
	SetBuffer ImageBuffer(tmpimage)
	For loop = 1 To 15 Step 2
		Color 0,0,0
		Line 0,loop,Len(txt)*8,loop
	Next
		
	SetBuffer buf
	DrawImage tmpimage,x,y
	FreeImage tmpimage
End Function

Function DoBigText(x,y,txt$,colour)
	buf = GraphicsBuffer()
	tmpimage = CreateImage(Len(txt)*8,8)
	SetBuffer ImageBuffer(tmpimage)

	DoText(0,0,txt,colour)
	SetBuffer buf
	ScaleImage tmpimage,1,2
	DrawImage tmpimage,x,y
	FreeImage tmpimage

End Function
	
Function DoText(x,y,txt$,colour)
	lstchar$ = ""
	For loop = 0 To Len(txt)-1
		ind$ = Mid$(txt$,loop+1,1)

		a = Asc(ind)-32
		If lstchar$="L" And ind$="T" Then x = x - 2

		DrawImageRect gpfont, x, y, a*8, colour*8, charwidth(a), 8
		x = x + charwidth(a)+1

		lstchar$=ind$
	Next
End Function


;.menus
;tmtext:	dc.b	'¹y',25,'¹d¹z¹f4TOURNAMENT MENU¹n¹b',65,50
;dc.b	'¹f2Play Next Match',10,10,'¹f5Start New Tournament¹j'
;	even
;ttres:	dc.l	ttgon
;ttgon:	dc.b	'¹b',15,225,'¹f7Main Menu',0

.menu_main
Data	"¹y",25,"¹d¹z¹f4GRAVITY POWER¹n¹c"
Data	"¹c¹y",75,"¹d¹z¹f1<  MAIN MENU  >¹n¹b",40,95
Data	"¹fFUse Joysticks To Select:¹b",65,113,"¹f2Play"
Data	"¹x",110,"Level Info","¹x",65
Data	10,10,"¹f3Tournament"
Data	10,10,"¹f4Load New Level",10,10,"¹f5Preferences",10,10
Data	"f7Help And Texts",10,1
Data	"¹f1Exit",0

;t_sinit:	dc.b	'¹y',40,'¹z¹f7',0

;	odd
;anders_txt:
;dc.b	'¹b',62,25,'¹d¹z¹f4 GRAVITY POWER ¹n¹c¹y',50
;dc.b	'¹z¹f5GP v1.20 (C) 1995 Bits Productions¹x',10,10,10
;dc.b	'¹z¹f4 CPU: 680'
;proc_type:	dc.b	'00 / Level Handler v'
;spec_ver:	dc.b	'x.xx / AGA Chipset: '
;aga_t:	dc.b	'No ',10,'¹c',10,10

;	incbin	"dh1:code/gp/gf-gfx/anders.txt"
;jb_txt:

;nttxt:	dc.b	'¹y',25,'¹D¹z¹f4NEW TOURNAMENT¹c¹n¹y',50,'¹z¹f5How Many Players?',0
;nttxt2:	dc.b	'¹y',50,'¹D¹z¹f2Input Your Name Player '
;ntt2b:	dc.b	'X¹p:¹c¹n',0

;winnertxt:
;	dc.b	'¹y',25,'¹D¹z¹f1- BATTLE REPORT -',10,10,'¹n¹x',40,'¹fFTotal Shots'
;	dc.b	' Fired:¹g'
;	even
;	dc.l	b_sh+1
;	dc.b	10,'¹x',40,'Battle took:¹g'
;	even
;	dc.l	b_ti
;	dc.b	' sec.¹c'
;	dc.b	'¹y',110,'¹D¹z¹f6And the WINNER is:¹c¹j'

;&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
;&&&&&&&&&&&&&&&&&&&&&&--Scripted Weapon Stuff--&&&&&&&&&&&&&&&&&&&&&&&&&&
;&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

.DoIncludes
Include "includes/debug.bb"
Include "includes/vport.bb"
Include "includes/stdlib.bb"
Include "includes/soundchan.bb"
Include "includes/quotes.bb"
Include "includes/weapons.bb"
Return

