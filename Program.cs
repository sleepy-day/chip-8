using static SDL2.SDL;
using System.Runtime.InteropServices;

int[] resolution = new int[] { 640, 480 };
int[] chip8Resolution = new int[] { 64, 32 };
int[] pixelSize = new int[] { resolution[0] / chip8Resolution[0], resolution[1] / chip8Resolution[1] };

IntPtr window;
IntPtr renderer;
IntPtr input;
bool running = true;
int arraySize;

Chip8 c8 = new Chip8();

c8.Initialize();

c8.LoadROM();

Setup();

int cycles = 1000 / 60;
int cycleDelay =  1000 / cycles;
long lastCycleTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

Console.WriteLine(cycles);

while (running) {
    PollEvents();
    HandleKeyInput();

    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    long dt = now - lastCycleTime;

    if (dt > cycleDelay) {
        lastCycleTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        c8.EmulateCycle();
    }

    Render();
}

CleanUp();


// SDL Functions
void Setup() {
    if (SDL_Init(SDL_INIT_VIDEO) < 0) {
        Console.WriteLine($"SDL failed to initialize: {SDL_GetError()}");
    }

    window = SDL_CreateWindow(
        "SDL Window", 
        SDL_WINDOWPOS_UNDEFINED, 
        SDL_WINDOWPOS_UNDEFINED, 
        resolution[0], 
        resolution[1], 
        SDL_WindowFlags.SDL_WINDOW_SHOWN
        );

    if (window == IntPtr.Zero) {
        Console.WriteLine($"Error creating window: {SDL_GetError()}");
    }

    renderer = SDL_CreateRenderer(
        window, 
        -1, 
        SDL_RendererFlags.SDL_RENDERER_ACCELERATED | 
        SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC
        );

    if (renderer == IntPtr.Zero) {
        Console.WriteLine($"Error creating renderer: {SDL_GetError()}");
    }

    input = SDL_GetKeyboardState(out arraySize);

    if (input == IntPtr.Zero) {
        Console.WriteLine($"Error getting keyboard state: {SDL_GetError()}");
    }
}

void PollEvents() {
    while (SDL_PollEvent(out SDL_Event e) == 1) {
        switch (e.type) {
            case SDL_EventType.SDL_QUIT:
            running = false;
            break;
        }
    }
}

void Render() {
    SDL_SetRenderDrawColor(renderer, 255, 163, 194, 255);
    SDL_RenderClear(renderer);

    SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

    for (int i = 0; i < c8.display.Length; i++) {
        if (c8.display[i]) {
            var rect = new SDL_Rect 
            { 
                x = (i % 64) * pixelSize[0],
                y = (i / 64) * pixelSize[1],
                w = pixelSize[0],
                h = pixelSize[1]
            };

            SDL_RenderFillRect(renderer, ref rect);
        }
    }

    SDL_RenderPresent(renderer);
}

void CleanUp() {
    SDL_DestroyRenderer(renderer);
    SDL_DestroyWindow(window);
    SDL_Quit();
}

void HandleKeyInput() {
    byte[] keyState = new byte[arraySize];
    Marshal.Copy(input, keyState, 0, arraySize);

    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_X] == 1) { c8.key[0x0] = 1; } else { c8.key[0x0] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_1] == 1) { c8.key[0x1] = 1; } else { c8.key[0x1] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_2] == 1) { c8.key[0x2] = 1; } else { c8.key[0x2] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_3] == 1) { c8.key[0x3] = 1; } else { c8.key[0x3] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_Q] == 1) { c8.key[0x4] = 1; } else { c8.key[0x4] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_W] == 1) { c8.key[0x5] = 1; } else { c8.key[0x5] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_E] == 1) { c8.key[0x6] = 1; } else { c8.key[0x6] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_A] == 1) { c8.key[0x7] = 1; } else { c8.key[0x7] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_S] == 1) { c8.key[0x8] = 1; } else { c8.key[0x8] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_D] == 1) { c8.key[0x9] = 1; } else { c8.key[0x9] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_Z] == 1) { c8.key[0xA] = 1; } else { c8.key[0xA] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_C] == 1) { c8.key[0xB] = 1; } else { c8.key[0xB] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_4] == 1) { c8.key[0xC] = 1; } else { c8.key[0xC] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_R] == 1) { c8.key[0xD] = 1; } else { c8.key[0xD] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_F] == 1) { c8.key[0xE] = 1; } else { c8.key[0xE] = 0; }
    if (keyState[(int)SDL_Scancode.SDL_SCANCODE_V] == 1) { c8.key[0xF] = 1; } else { c8.key[0xF] = 0; }
}