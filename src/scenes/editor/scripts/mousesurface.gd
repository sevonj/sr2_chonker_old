extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var active = false

# Called when the node enters the scene tree for the first time.
func _ready():
	pass #connect("gui_input", self, "_on_gui_input")


func _process(_delta):
	if Input.is_mouse_button_pressed(BUTTON_LEFT):
		active = true
	else: active = false


func _input(event: InputEvent):
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_MIDDLE:
			print("mid")
