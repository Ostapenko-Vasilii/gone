using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using gone.Objects;

namespace gone.UI;

public class GameHud
{
    private readonly int _screenWidth;
    private SpriteFont _font;

    private const int PanelWidth = 300;
    private const int PanelHeight = 200;
    private const int Padding = 16;
    private const int BarHeight = 20;
    private const int BarWidth = 263;

    public GameHud(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
    }

    public void LoadContent(ContentManager contentManager)
    {
        try
        {
                _font = contentManager.Load<SpriteFont>("Arial");
        }
        catch
        {
            _font = null;
        }
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Building baseBuilding, int score, int towerCost, float coolingProgress)
    {
        var panelX = _screenWidth - PanelWidth - Padding;
        var panelY = Padding;

        DrawPanel(spriteBatch, graphicsDevice, panelX, panelY);
        DrawHealthBar(spriteBatch, graphicsDevice, baseBuilding, panelX, panelY);
        if (coolingProgress > 0)
            DrawCoolingBar(spriteBatch, graphicsDevice, coolingProgress, panelX, panelY);
            
        if (_font != null)
        {
            DrawHealthText(spriteBatch, baseBuilding, panelX, panelY);
            DrawScoreText(spriteBatch, score, panelX, panelY);
            DrawTowerCostText(spriteBatch, towerCost, panelX, panelY);
        }
    }

    private void DrawPanel(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, int x, int y)
    {
        var panelTexture = new Texture2D(graphicsDevice, PanelWidth, PanelHeight);
        var panelData = new Color[PanelWidth * PanelHeight];

        for (var i = 0; i < panelData.Length; i++)
            panelData[i] = new Color(0, 0, 0, 200);

        panelTexture.SetData(panelData);
        spriteBatch.Draw(panelTexture, new Rectangle(x, y, PanelWidth, PanelHeight), Color.White);

        var borderTexture = new Texture2D(graphicsDevice, 1, 1);
        borderTexture.SetData(new[] { Color.White });

        spriteBatch.Draw(borderTexture, new Rectangle(x, y, PanelWidth, 2), Color.White);
        spriteBatch.Draw(borderTexture, new Rectangle(x, y + PanelHeight - 2, PanelWidth, 2), Color.White);
        spriteBatch.Draw(borderTexture, new Rectangle(x, y, 2, PanelHeight), Color.White);
        spriteBatch.Draw(borderTexture, new Rectangle(x + PanelWidth - 2, y, 2, PanelHeight), Color.White);
    }

    private void DrawHealthBar(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Building baseBuilding, int panelX, int panelY)
    {
        var barX = panelX + Padding;
        var barY = panelY + 40;

        var healthPercent = baseBuilding.Health / baseBuilding.MaxHealth;
        var filledWidth = (int)(BarWidth * healthPercent);

        var backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
        backgroundTexture.SetData(new[] { Color.DarkRed });
        spriteBatch.Draw(backgroundTexture, new Rectangle(barX, barY, BarWidth, BarHeight), Color.White);

        var color = healthPercent > 0.5f ? Color.LimeGreen : (healthPercent > 0.25f ? Color.Yellow : Color.Red);
        var fillTexture = new Texture2D(graphicsDevice, 1, 1);
        fillTexture.SetData(new[] { color });
        spriteBatch.Draw(fillTexture, new Rectangle(barX, barY, filledWidth, BarHeight), Color.White);
    }
    
    private void DrawCoolingBar(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, float coolingProgress, int panelX, int panelY)
    {
        var barX = panelX + Padding;
        var barY = panelY + 65;

        var filledWidth = (int)(BarWidth * coolingProgress);

        var backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
        backgroundTexture.SetData(new[] { Color.DarkBlue });
        spriteBatch.Draw(backgroundTexture, new Rectangle(barX, barY, BarWidth, BarHeight / 2), Color.White);

        var fillTexture = new Texture2D(graphicsDevice, 1, 1);
        fillTexture.SetData(new[] { Color.Aqua });
        spriteBatch.Draw(fillTexture, new Rectangle(barX, barY, filledWidth, BarHeight / 2), Color.White);
    }

    private void DrawHealthText(SpriteBatch spriteBatch, Building baseBuilding, int panelX, int panelY)
    {
        var textX = panelX + Padding;
        var textY = panelY + Padding;

        var healthText = $"Base Health: {(int)baseBuilding.Health} / {(int)baseBuilding.MaxHealth}";
        spriteBatch.DrawString(_font, healthText, new Vector2(textX, textY), Color.White);
    }
    
    private void DrawScoreText(SpriteBatch spriteBatch, int score, int panelX, int panelY)
    {
        var textX = panelX + Padding;
        var textY = panelY + 100;

        var scoreText = $"Score: {score}";
        spriteBatch.DrawString(_font, scoreText, new Vector2(textX, textY), Color.White);
    }
    
    private void DrawTowerCostText(SpriteBatch spriteBatch, int towerCost, int panelX, int panelY)
    {
        var textX = panelX + Padding;
        var textY = panelY + 140;

        var costText = $"Tower Cost: {towerCost}";
        spriteBatch.DrawString(_font, costText, new Vector2(textX, textY), Color.White);
    }
}