from random import *
from pygame import *
import math
import collections

class ShatteredSprite:
    class Config:
        def __init__(self):
            self.seed = 1337
            self.image = None
            self.r_spread_min = 0.1
            self.r_spread_max = 0.2
            self.n_pts_per_split = 2
            self.outbound_range = [0, 360]

            self.wait_time = 1.0
            self.explode_time = 5.0
            self.explode_factor = 35.0
            self.explode_scale = 4.0


    class __Piece:
        def __flood(self, start_flood_pos, img):
            def in_range(pt):
                return 0 <= pt[0] and pt[0] < img.get_width() and 0 <= pt[1] and pt[1] < img.get_height()

            backup = img.copy()
            q = collections.deque([start_flood_pos])
            res = []
            w_range = [start_flood_pos[0], start_flood_pos[0]]
            h_range = [start_flood_pos[1], start_flood_pos[1]]
            while q:
                cur = q.popleft()
                res.append(cur)
                w_range = min(w_range[0], cur[0]), max(w_range[1], cur[0])
                h_range = min(h_range[0], cur[1]), max(h_range[1], cur[1])
                for n in [(cur[0] - 1, cur[1]), (cur[0] + 1, cur[1]), (cur[0], cur[1] - 1), (cur[0], cur[1] + 1)]:
                    if in_range(n) and img.get_at(n)[3] != 0:
                        res.append(n)
                        q.append(n)
                        img.set_at(n, (0,0,0,0))

            self.surf = Surface((w_range[1] - w_range[0], h_range[1] - h_range[0]), SRCALPHA)
            self.surf.fill((0,0,0,0))
            self.offset = (self.surf.get_width() / 2, self.surf.get_height() / 2)
            self.position = (w_range[0] + self.offset[0], h_range[0] + self.offset[1])
            for c in res:
                local_c = (c[0] - w_range[0], c[1] - h_range[0])
                self.surf.set_at(local_c, backup.get_at(c))

        def __init__(self, start_flood_pos, img):
            self.__flood(start_flood_pos, img)
            dx = self.position[0] - img.get_width()/2
            dy = self.position[1] - img.get_height()/2
            self.r = math.hypot(dx, dy)
            self.direction = (dx / self.r, dy / self.r)

        def blit(self, surf, pos, d):
            px = int(self.direction[0] * d)
            py = int(self.direction[1] * d)
            surf.blit(self.surf, (pos[0] + self.position[0] + px - self.offset[0], pos[1] + self.position[1] + py - self.offset[1]))

    def __shatter(self):
        config = self.config
        self.__image_copy = Surface((config.image.get_width(), config.image.get_height()), SRCALPHA)
        self.__image_copy.blit(config.image, (0, 0))
        img = self.__image_copy
        center = (img.get_width() / 2, img.get_height() / 2)
        r = math.hypot(img.get_width() / 2.0, img.get_height() / 2.0)
        r_spread_min = int(config.r_spread_min * r)
        r_spread_max = int(config.r_spread_max * r)

        def in_range(pt):
            return 0 <= pt[0] and pt[0] < img.get_width() and 0 <= pt[1] and pt[1] < img.get_height()

        def scanline(s_pos, e_pos):
            dx = e_pos[0] - s_pos[0]
            dy = e_pos[1] - s_pos[1]
            l = math.ceil(math.hypot(dx, dy))
            dsx = dx / l
            dsy = dy / l

            lx, ly = [-1, -1]
            for i in range(int(l)):
                x = int(s_pos[0] + dsx * i)
                y = int(s_pos[1] + dsy * i)
                if lx == x and ly == y:
                    continue
                lx, ly = x, y
                if not in_range([x, y]) or (img.get_at((x, y))[3] == 0 and i != 0):
                    return False
                img.set_at((x,y), (0,0,0,0))
            return True

        def rotate(r, ang, p_0 = [0, 0]):
            rang = math.radians(ang)
            return map(int, [p_0[0] + r * math.cos(rang), p_0[1] + r * math.sin(rang)])

        def shatter(outbound_range=config.outbound_range, prev_pt=center, split_number=config.n_pts_per_split):
            if not in_range(prev_pt):
                return
            prev_radial_dist = math.hypot(prev_pt[0] - center[0], prev_pt[1] - center[1])
            drange = outbound_range[1] - outbound_range[0] + 1
            
            for i in range(split_number):
                r_start = drange / split_number * i + outbound_range[0]
                r_end = drange / split_number * (i + 1) + outbound_range[0]
                #while True:
                rand_ang = randint(r_start, r_end)
                rand_rad = randint(r_spread_min, r_spread_max)
                pt = rotate(rand_rad, rand_ang, prev_pt)
                
                if scanline(prev_pt, pt):
                    shatter([rand_ang - 90, rand_ang + 90], pt, split_number)
        shatter()

    def __init__(self, config):
        self.config = config
        seed(self.config.seed)
        self.__shatter()
        self.piece_interp_function = lambda t, r: ((r * (self.config.explode_scale - 1)) *
                                                   (-1.0 / (t * self.config.explode_factor /
                                                            (self.config.explode_time)
                                                            + 1)
                                                    + 1.0))
        self.width = int(self.piece_interp_function(self.config.explode_time, self.config.image.get_width()) + self.config.image.get_width())
        self.height = int(self.piece_interp_function(self.config.explode_time, self.config.image.get_height()) + self.config.image.get_height())
        self.pieces = []
        img = self.__image_copy
        for i in range(img.get_width()):
            for j in range(img.get_height()):
                if img.get_at((i,j))[3] != 0:
                    self.pieces.append(ShatteredSprite.__Piece((i,j), img))

        self.offset = (img.get_width() / 2, img.get_height() / 2)

    def blit(self, surf, pos, t):
        if t < self.config.wait_time:
            surf.blit(self.config.image, (pos[0]  - self.offset[0], pos[1] - self.offset[1]))
        else:
            for p in self.pieces:
                d = self.piece_interp_function(t - self.config.wait_time, p.r)
                p.blit(surf, (pos[0]  - self.offset[0], pos[1] - self.offset[1]), d)

conf = ShatteredSprite.Config()
conf.seed = "EXPLOOSIONNN!"
conf.image = image.load("Green Candy.png")
conf.image = transform.scale(conf.image, (conf.image.get_width(), conf.image.get_height()))
conf.explode_scale = 2.0
conf.r_spread_min = 0.1
conf.r_spread_max = 0.2
conf.wait_time = 0
conf.explode_time = 1
conf.explode_factor = 25

ss = ShatteredSprite(conf)

def main():
    screen = display.set_mode((ss.width, ss.height),
                              SRCALPHA)

    center = (screen.get_width() / 2, screen.get_height() / 2)
    running = True
    t = 0
    clock = time.Clock()

    while running:
        for evt in event.get():
            if evt.type == QUIT:
                running = False

        t = (t + 1.0/60) % (conf.explode_time + conf.wait_time)
        
        screen.fill((0,0,0))
        ss.blit(screen, center, t)

        display.flip()
        clock.tick(60)
    quit()

def genSpriteMap(frames):
    w = int(math.sqrt(frames))
    h = int(math.ceil(frames / float(w)))
    surf = Surface((ss.width * w, ss.height * h), SRCALPHA)
    center = (ss.width / 2, ss.height / 2)
    surf.fill((255,255,255,0))
    for i in range(h):
        for j in range(w):
            t = i * w + j
            if t >= frames:
                continue
            ss.blit(surf, (j * ss.width + center[0], i * ss.height + center[1]), 1.0 / frames * t)
    image.save(surf, "sheet.png")
    quit()

main()
genSpriteMap(60)
