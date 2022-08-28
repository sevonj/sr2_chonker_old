extends Spatial


# Declare member variables here. Examples:
var look_sensitivity = Vector2(.005, .005)

var look_delta: Vector2
var fly_active = false

onready var pivot = $pivot
onready var cam = $pivot/cam

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _input(event):
	if event is InputEventMouseMotion:
		look_delta += event.relative * look_sensitivity
		get_viewport().set_input_as_handled()
	elif event is InputEventMouseButton:
		if event.button_index == BUTTON_WHEEL_UP: cam.transform.origin.y *= .9
		elif event.button_index == BUTTON_WHEEL_DOWN: cam.transform.origin.y /= .9
		
func _process(delta):
	
	
	if Input.is_key_pressed(KEY_TAB): #fly_active:
		rotate_y(look_delta.x)
		pivot.rotate_x(look_delta.y)
		
		Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
		var input = Vector2.ZERO
		if Input.is_key_pressed(KEY_W): input.y += 1
		if Input.is_key_pressed(KEY_S): input.y -= 1
		if Input.is_key_pressed(KEY_A): input.x += 1
		if Input.is_key_pressed(KEY_S): input.x -= 1
	else:
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	look_delta = Vector2.ZERO
