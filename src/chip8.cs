using System;

class Chip8 {
    byte[] font = new byte[] {
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80
    };

    const int WIDTH = 64;
    const int HEIGHT = 32;

    public bool[] display = new bool[64 * 32];
    public byte[] key = new byte[16];

    byte[] RAM = new byte[4096];
    uint[] stack = new uint[16];
    byte[] V = new byte[16];
    bool drawFlag = false;
    uint opcode;
    uint PC;
    uint I;
    uint SP;
    uint delayTimer;
    uint soundTimer;

    int reg1;
    int reg2;

    public void Initialize() {
        PC = 0x200;
        opcode = 0;
        I = 0;
        SP = 0;

        Array.Fill<bool>(display, false);
        Array.Fill<byte>(RAM, 0x00);
        Array.Fill<byte>(V, 0x00);
        Array.Fill<byte>(key, 0x00);
        Array.Fill<uint>(stack, 0);

        for (int i = 0; i < font.Length; i++) {
            RAM[0x050 + i] = font[i];
        }

        delayTimer = 0;
        soundTimer = 0;
    }

    public void LoadROM() {
        Console.WriteLine("Enter name of ROM: ");
        //string ROM = Console.ReadLine();

        byte[] temp = File.ReadAllBytes("./6-keypad.ch8");

        for (int i = 0; i < temp.Length; i++) {
            RAM[0x200 + i] = temp[i];
        }

        for (int i = 0; i < RAM.Length; i++) {
            Console.Write(RAM[i]);
        }
    }

    public void EmulateCycle() {
        opcode = (uint)RAM[PC] << 8 | RAM[PC+1];
        
        Console.WriteLine("opcode: {0:X}", opcode);

        reg1 = (int)((opcode & 0xF00) >> 8);
        reg2 = (int)((opcode & 0xF0) >> 4);

        if (delayTimer > 0) {
            --delayTimer;
        }

        if (soundTimer > 0) {
            --soundTimer;
        }

        switch (opcode & 0xF000) {
            case 0x0000:
                switch (opcode) {
                    case 0x00E0:
                        ClearScreen();
                        break;
                    case 0x00EE:
                        Ret();
                        break;
                    default:
                        break;
                }
                break;
            case 0x1000:
                JumpTo();
                break;
            case 0x2000:
                CallFunc();
                break;
            case 0x3000:
                SkipIfV();
                break;
            case 0x4000:
                SkipIfNotV();
                break;
            case 0x5000:
                SkipIfVEqual();
                break;
            case 0x6000:
                SetV();
                break;
            case 0x7000:
                AddV();
                break;
            case 0x8000:
                switch (opcode & 0x000F) {
                    case 0x0000:
                        CopyV();
                        break;
                    case 0x0001:
                        OrV();
                        break;
                    case 0x0002:
                        AndV();
                        break;
                    case 0x0003:
                        XorV();
                        break;
                    case 0x0004:
                        AddVF();
                        break;
                    case 0x0005:
                        SubV();
                        break;
                    case 0x0006:
                        ShrV();
                        break;
                    case 0x0007:
                        SubNV();
                        break;
                    case 0x000E:
                        ShlV();
                        break;
                }
                break;
            case 0x9000:
                SNE();
                break;
            case 0xA000:
                LDI();
                break;
            case 0xB000:
                JumpToV0();
                break;
            case 0xC000:
                Rnd();
                break;
            case 0xD000:
                Draw();
                break;
            case 0xE000:
                switch (opcode & 0x00FF) {
                    case 0x009E:
                        SkipIfKeyPressed();
                        break;
                    case 0x00A1:
                        SkipIfKeyNotPressed();
                        break;
                }
                break;
            case 0xF000:
                switch (opcode & 0x00FF) {
                    case 0x0007:
                        StoreDelayTimer();
                        break;
                    case 0x000A:
                        StoreKeyPress();
                        break;
                    case 0x0015:
                        SetDelayTimer();
                        break;
                    case 0x0018:
                        SetSoundTimer();
                        break;
                    case 0x001E:
                        SetAndAddI();
                        break;
                    case 0x0029:
                        SetISpriteLocation();
                        break;
                    case 0x0033:
                        StoreBCD();
                        break;
                    case 0x0055:
                        StoreV();
                        break;
                    case 0x0065:
                        RetrieveV();
                        break;
                }
                break;
            default:
                Console.WriteLine("Unknown opcode: " + opcode);
                break;
        }
        PC += 2;

        for (int i = 0; i < V.Length; i++) {
            Console.Write("V" + i + " : " + V[i] + " ");
        }
        Console.WriteLine();
    }

    private void ClearScreen() {
        Array.Fill<bool>(display, false);
    }

    private void Ret() {
        PC = stack[SP];
        SP--;
    }

    private void JumpTo() {
        PC = opcode & 0x0FFF;
        PC -= 2;
    }

    private void CallFunc() {
        SP++;
        stack[SP] = PC;
        PC = opcode & 0xFFF;
        PC -= 2;
    }

    private void SkipIfV() {
        int compareValue = (int)(opcode & 0xFF);
        if (V[reg1] == compareValue) {
            PC += 2;
        }
    }

    private void SkipIfNotV() {
        uint compareValue = opcode & 0xFF;
        if (V[reg1] != compareValue) {
            PC += 2;
        }
    }

    private void SkipIfVEqual() {
        if (V[reg1] == V[reg2]) {
            PC += 2;
        }
    }

    private void SetV() {        
        uint val = opcode & 0xFF;
        V[reg1] = (byte)val;
    }

    private void AddV() {
        uint val = V[reg1] + (opcode & 0xFF);
        V[reg1] = (byte)val;
    }

    private void CopyV() {
        V[reg1] = V[reg2];
    }

    private void OrV() {
        V[reg1] = (byte)(V[reg1] | V[reg2]);
    }

    private void AndV() {
        V[reg1] = (byte)(V[reg1] & V[reg2]);
    }

    private void XorV() {
        V[reg1] = (byte)(V[reg1] ^ V[reg2]);
    }

    private void AddVF() {
        uint val = (uint)(V[reg1] + V[reg2]);

        Console.WriteLine(val);

        if (val > 0xFF) {
            V[reg1] = (byte)(val & 0xFF);
            V[0xF] = 1;
        } else {
            V[reg1] = (byte)val;
            V[0xF] = 0;
        }
    }

    private void SubV() {
        byte result = (byte)(V[reg1] - V[reg2]);
        
        if (V[reg1] < V[reg2]) {
            V[reg1] = result;
            V[0xF] = 0;
        } else {
            V[reg1] = result;
            V[0xF] = 1;
        }
    }

    private void ShrV() {
        V[reg1] = V[reg2];
        int carry = 0;
        if ((V[reg1] & 0x1) == 1) {
            carry = 1;
        }

        V[reg1] = (byte)(V[reg1] >> 1);
        V[0xF] = (byte)carry;
    }

    private void SubNV() {
        byte result = (byte)(V[reg2] - V[reg1]);

        if (V[reg2] < V[reg1]) {
            V[reg1] = result;
            V[0xF] = 0;
        } else {
            V[reg1] = result;
            V[0xF] = 1;
        }
    }

    private void ShlV() {
        V[reg1] = V[reg2];

        if (((V[reg1] & 0x80) >> 7) == 1) {
            V[reg1] = (byte)(V[reg1] << 1);
            V[0xF] = 1;
        } else {
            V[reg1] = (byte)(V[reg1] << 1);
            V[0xF] = 0;
        }
    }

    private void SNE() {
        if (V[reg1] != V[reg2]) {
            PC += 2;
        }
    }

    private void LDI() {
        uint val = opcode & 0x0FFF;
        I = val;
    }

    private void JumpToV0() {
        PC = (opcode & 0xFFF) + V[0];
    }

    private void Rnd() {
        byte[] b = new byte[1];
        Random random = new Random();
        random.NextBytes(b);

        V[reg1] = (byte)(V[reg1] & b[0]);
    }

    private void Draw() {
        uint x = V[((opcode & 0x0F00) >> 8)];
        uint y = V[((opcode & 0x00F0) >> 4)];
        uint height = opcode & 0x000F;
        uint pixel;

        Console.WriteLine("Vx: " + x + " Vy: " + y);

        x = x % WIDTH;
        y = y % HEIGHT;

        Console.WriteLine("after mod Vx: " + x + " Vy: " + y);

        V[0xF] = 0;
        for (int yline = 0; yline < height; yline++) {
            pixel = RAM[I + yline];
            for (int xline = 0; xline < 8; xline++) {
                if ((pixel & (0x80 >> xline)) != 0) {
                    if(display[(x + xline + ((y + yline) * 64))] == true) {
                        V[0xF] = 1;
                    }
                    display[x + xline + ((y + yline) * 64)] ^= true;
                }
            }
        }

        drawFlag = true;
    }

    private void SkipIfKeyPressed() {
        if (key[V[(opcode & 0x0F00) >> 8]] != 0) {
            PC += 2;
        }
    }

    private void SkipIfKeyNotPressed() {
        if (key[V[(opcode & 0x0F00) >> 8]] != 1) {
            PC += 2;
        }
    }

    private void StoreDelayTimer() {
        V[reg1] = (byte)delayTimer;
    }

    private void StoreKeyPress() {
        bool keyPressed = false;

        for (int i = 0; i < key.Length; i++) {
            if (key[i] == 1) {
                V[reg1] = (byte)i;
                keyPressed = true;
            }
        }

        if (!keyPressed) {
            PC -= 2;
        }
    }

    private void SetDelayTimer() {
        delayTimer = V[reg1];
    }

    private void SetSoundTimer() {
        soundTimer = V[reg1];
    }

    private void SetAndAddI() {
        I += V[reg1];
    }

    private void SetISpriteLocation() {
        I = V[reg1];
    }

    private void StoreBCD() {
        Console.WriteLine("BCD V: " + V[reg1]);
        RAM[I] = (byte)(V[reg1] / 100);
        RAM[I + 1] = (byte)((V[reg1] % 100) / 10);
        RAM[I + 2] = (byte)((V[reg1] % 10));
        Console.WriteLine("1: " + (V[reg1] / 100));
        Console.WriteLine("2: " + ((V[reg1] % 100) / 10));
        Console.WriteLine("3: " + ((V[reg1] % 10)));
        Console.WriteLine("1: " + RAM[I]);
        Console.WriteLine("2: " + RAM[(I + 1)]);
        Console.WriteLine("3: " + RAM[(I + 2)]);
    }

    private void StoreV() {
        for (int i = 0; i <= reg1; i++) {
            RAM[I + i] = V[i];
        }
        
    }

    private void RetrieveV() {
        for (int i = 0; i <= reg1; i++) {
            V[i] = RAM[I + i];
        }
        
    }
}   