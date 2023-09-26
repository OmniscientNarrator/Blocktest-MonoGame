using Blocktest.Rendering;
using Microsoft.Xna.Framework.Input;
namespace Blocktest.Scenes; 

public class GameScene : Scene {
    private readonly BlocktestGame _game;
    private readonly SpriteBatch _spriteBatch;
    
    bool latch = false; //latch for button pressing
    private bool latchBlockSelect = false; //same but for block selection
    bool buildMode = true; //true for build, false for destroy
    private int blockSelected = 0; //ID of the block to place
    private Camera _camera;
    
    public void Update(GameTime gameTime) {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape)) {
                _game.Exit();
            }

            //press E to toggle build/destroy
            if (keyState.IsKeyUp(Keys.E))
            {
	            latch = false;
            } 
            else if (latch == false)
            {
	            buildMode = !buildMode;
	            latch = true;
            }

            //Q changes which block you have selected
            if (keyState.IsKeyUp(Keys.Q))
            {
	            latchBlockSelect = false;
            }
            else if (latchBlockSelect == false)
            {
	            blockSelected++;
	            if (blockSelected >= BlockManager.AllBlocks.Length)
	            {
		            blockSelected = 0;
	            }

	            latchBlockSelect = true;
            }

            var moveValue = 2.5f;
            if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift)) {
                moveValue *= 4;
            }
            
            if (keyState.IsKeyDown(Keys.A)) {
                _camera.Position.X -= moveValue;
            } else if (keyState.IsKeyDown(Keys.D)) {
                _camera.Position.X += moveValue;
            } 
            
            if (keyState.IsKeyDown(Keys.W)) {
                _camera.Position.Y += moveValue;
            } else if (keyState.IsKeyDown(Keys.S)) {
                _camera.Position.Y -= moveValue;
            }

            if (!_camera.RenderLocation.Contains(mouseState.Position)) {
                return;
            }

            var mousePos = _camera.CameraToWorldPos(new(mouseState.X, mouseState.Y));
            //build and destroy mode
            if (buildMode)
            {
	            if(mouseState.LeftButton == ButtonState.Pressed)
	            {
	                BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[blockSelected], true,
	                    new Vector2Int(MathHelper.Clamp(mousePos.X / Globals.gridSize.X, 0, Globals.maxX), 
		                    MathHelper.Clamp(mousePos.Y / Globals.gridSize.Y, 0, Globals.maxY)));
	            } else if (mouseState.RightButton == ButtonState.Pressed) {
		            BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[blockSelected], false,
			            new Vector2Int(MathHelper.Clamp(mousePos.X / Globals.gridSize.X, 0, Globals.maxX), 
				            MathHelper.Clamp(mousePos.Y / Globals.gridSize.Y, 0, Globals.maxY)));
	            }
            }
            else 
            {
	            if(mouseState.LeftButton == ButtonState.Pressed)
	            {
		            BuildSystem.BreakBlockCell( true,
			            new Vector2Int(MathHelper.Clamp(mousePos.X / Globals.gridSize.X, 0, Globals.maxX), 
				            MathHelper.Clamp(mousePos.Y / Globals.gridSize.Y, 0, Globals.maxY)));
	            } else if (mouseState.RightButton == ButtonState.Pressed) {
		            BuildSystem.BreakBlockCell( false,
			            new Vector2Int(MathHelper.Clamp(mousePos.X / Globals.gridSize.X, 0, Globals.maxX), 
				            MathHelper.Clamp(mousePos.Y / Globals.gridSize.Y, 0, Globals.maxY)));
	            }
            } 
    }

    public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice) {
        
        
        _camera.Draw(graphicsDevice, _spriteBatch);

        const bool pixelPerfect = false;
        
        var destinationRectangle = pixelPerfect ? GetPixelPerfectRect() : GetFitRect();
        _camera.RenderLocation = destinationRectangle;

        graphicsDevice.Clear(Color.DarkGray);
        
        _spriteBatch.Begin(samplerState: pixelPerfect ? SamplerState.PointClamp : null);
        _spriteBatch.Draw(_camera.RenderTarget, destinationRectangle, Color.White);
        _spriteBatch.End();
    }

    private Rectangle GetPixelPerfectRect() {
        int multiplier = int.Min(_game.GraphicsDevice.Viewport.Height / _camera.RenderTarget.Height,
            _game.GraphicsDevice.Viewport.Width / _camera.RenderTarget.Width);

        int width = _camera.RenderTarget.Width * multiplier;
        int height = _camera.RenderTarget.Height * multiplier;

        int x = (_game.GraphicsDevice.Viewport.Width - width) / 2;
        int y = (_game.GraphicsDevice.Viewport.Height - height) / 2;
            
        return new Rectangle(x, y, width, height);
    }

    private Rectangle GetFitRect() {
        float aspectRatio = (float)_game.GraphicsDevice.Viewport.Width / _game.GraphicsDevice.Viewport.Height;
        float renderTargetAspectRatio = (float)_camera.RenderTarget.Width / _camera.RenderTarget.Height;

        int width, height;
        if (aspectRatio > renderTargetAspectRatio) {
            width = (int)(_game.GraphicsDevice.Viewport.Height * renderTargetAspectRatio);
            height = _game.GraphicsDevice.Viewport.Height;
        }
        else {
            width = _game.GraphicsDevice.Viewport.Width;
            height = (int)(_game.GraphicsDevice.Viewport.Width / renderTargetAspectRatio);
        }
        
        int x = (_game.GraphicsDevice.Viewport.Width - width) / 2;
        int y = (_game.GraphicsDevice.Viewport.Height - height) / 2;
            
        return new Rectangle(x, y, width, height);
    }

    
    
    public GameScene(BlocktestGame game) {
        _spriteBatch = new SpriteBatch(game.GraphicsDevice);
        _game = game;
        _camera = new Camera(new Vector2(0, 0), new Vector2(512, 256), _game.GraphicsDevice);
        
        Globals.BackgroundTilemap = new Tilemap(Globals.maxX, Globals.maxY, _camera);
        Globals.ForegroundTilemap = new Tilemap(Globals.maxX, Globals.maxY, _camera);

        for (int i = 0; i < Globals.maxX; i++) {
            BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[1], true, new Vector2Int(i, 5));
            BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[0], true, new Vector2Int(i, 4));
            BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[0], true, new Vector2Int(i, 3));
            BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[0], true, new Vector2Int(i, 2));
            BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[0], true, new Vector2Int(i, 1));
            BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[2], true, new Vector2Int(i, 0));
        }
            
        BuildSystem.PlaceBlockCell(BlockManager.AllBlocks[0], true, new Vector2Int(20, 20));
    }
}